namespace Core.Application;

/// <summary>
/// 登录与登出应用服务
/// </summary>
public class LoginAuthService(
    JwtSigningSettings jwtSigningSettings,
    IBaseRepo<SysUser> userRepo,
    IBaseRepo<SysUserRole> userRoleRepo,
    IHttpContextAccessor httpContextAccessor,
    IBaseRepo<SysRecordLogin> loginRecordRepo,
    IBaseRepo<SysRole> roleRepo,
    ISmsCodeVerification smsCodeVerification,
    IHttpContextUser contextUser,
    ITairProvider tairProvider) : ILoginAuthService
{
    private const string UserAgentHeaderName = "User-Agent";
    private const int MaxLoginFailCount = 10;
    private const int LockoutMinutes = 15;
    private const string LoginFailCachePrefix = "login_fail_";

    /// <summary>
    /// 从 ITairProvider 读取失败记录，反序列化 JSON
    /// </summary>
    private static LoginFailRecord? ReadFailRecord(ITairProvider tair, string key)
    {
        if (tair.TryGetString(key, out var json) && !string.IsNullOrWhiteSpace(json))
            return JsonSerializer.Deserialize<LoginFailRecord>(json);
        return null;
    }

    /// <summary>
    /// 将失败记录序列化 JSON 写入 ITairProvider
    /// </summary>
    private static void WriteFailRecord(ITairProvider tair, string key, LoginFailRecord record, TimeSpan expiry) =>
        tair.SetString(key, JsonSerializer.Serialize(record), expiry);

    sealed record LoginFailRecord(int FailCount, DateTime? LockoutEndTime);

    /// <summary>
    /// 用户名密码登录并签发 JWT
    /// </summary>
    public async Task<CustomApiResponse<string>> LoginByPasswordAsync(LoginByPasswordReq request)
    {
        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            return CustomApiResponse<string>.Fail(GlobalConstVars.AuthEmptyFailure);

        var user = await userRepo.GetFirstAsync(p => p.UserName == request.UserName);
        if (user is null)
        {
            await LogLoginAsync(request.UserName, LoginRecordType.登录失败, "用户名或密码错误");
            return CustomApiResponse<string>.Fail("用户名或密码错误");
        }

        // 缓存锁定检查
        var cacheKey = LoginFailCachePrefix + request.UserName;
        var record = ReadFailRecord(tairProvider, cacheKey);

        if (record is { LockoutEndTime: not null } && record.LockoutEndTime > DateTime.Now)
        {
            var remainMinutes = (int)(record.LockoutEndTime.Value - DateTime.Now).TotalMinutes + 1;
            await LogLoginAsync(request.UserName, LoginRecordType.登录失败, "账户已被锁定");
            return CustomApiResponse<string>.Fail($"账户已锁定，请{remainMinutes}分钟后再试");
        }

        if (!PasswordHelper.TryVerify(request.Password, user.Password, out var needsUpgrade))
        {
            // 失败计数 +1
            var newCount = (record?.FailCount ?? 0) + 1;
            DateTime? lockoutEnd = newCount >= MaxLoginFailCount
                ? DateTime.Now.AddMinutes(LockoutMinutes)
                : null;

            var expiry = lockoutEnd ?? DateTime.Now.AddMinutes(LockoutMinutes);
            WriteFailRecord(tairProvider, cacheKey, new LoginFailRecord(newCount, lockoutEnd), expiry - DateTime.Now);

            var remainChances = MaxLoginFailCount - newCount;
            var msg = remainChances > 0
                ? $"用户名或密码错误，还剩{remainChances}次机会"
                : "用户名或密码错误，账户已锁定，请15分钟后再试";
            await LogLoginAsync(request.UserName, LoginRecordType.登录失败, msg);
            return CustomApiResponse<string>.Fail(msg);
        }

        // 登录成功，清除缓存
        tairProvider.Remove(cacheKey);

        if (needsUpgrade)
        {
            user.Password = PasswordHelper.Hash(request.Password);
            await userRepo.EditAsync(user);
        }

        var (success, message, token, _) = await BuildLoginResultAsync(user);
        if (!success)
        {
            await LogLoginAsync(request.UserName, LoginRecordType.登录失败, message);
            return CustomApiResponse<string>.Fail(message);
        }

        await LogLoginAsync(request.UserName, LoginRecordType.登录成功, LoginRecordType.登录成功.ToString());

        return CustomApiResponse<string>.Ok(GlobalConstVars.AuthSuccess, token);
    }

    /// <summary>
    /// 手机号短信登录并签发 JWT
    /// </summary>
    public async Task<CustomApiResponse<string>> LoginByPhoneAsync(LoginByPhoneReq request)
    {
        var user = await userRepo.GetFirstAsync(p => p.Phone == request.PhoneNumber);
        if (user is null)
            return CustomApiResponse<string>.Fail(GlobalConstVars.PhoneNotRegistered);

        if (!smsCodeVerification.Verify(request.PhoneNumber, request.SmsCode))
            return CustomApiResponse<string>.Fail(GlobalConstVars.PhoneSmsInvalid);

        var (success, message, token, _) = await BuildLoginResultAsync(user);
        if (!success)
        {
            await LogLoginAsync(user.UserName, LoginRecordType.登录失败, message);
            return CustomApiResponse<string>.Fail(message);
        }

        await LogLoginAsync(user.UserName, LoginRecordType.登录成功, LoginRecordType.登录成功.ToString());

        return CustomApiResponse<string>.Ok(GlobalConstVars.AuthSuccess, token);
    }

    /// <summary>
    /// 记录退出日志
    /// </summary>
    public async Task<CustomApiResponse> LogoutAsync()
    {
        if (contextUser.IsAuthenticated())
        {
            var log = new SysRecordLogin
            {
                UserName = contextUser.UserName,
                Os = RuntimeInformation.OSDescription,
                OperType = LoginRecordType.退出登录,
                Comments = LoginRecordType.退出登录.ToString(),
                LoginSource = LoginSource.平台登录.ToString(),
                CreateTime = DateTime.Now
            };

            if (httpContextAccessor.HttpContext is not null)
                log.Browser = httpContextAccessor.HttpContext.Request.Headers[UserAgentHeaderName];

            await loginRecordRepo.InsertAsync(log);
        }

        return CustomApiResponse.Ok(GlobalConstVars.AuthLogoutSuccess);
    }

    /// <summary>
    /// 校验用户状态与角色后签发 JWT
    /// </summary>
    private async Task<(bool success, string message, string token, List<SysRole> roles)> BuildLoginResultAsync(SysUser user)
    {
        if (user.Status == UserStatus.停用)
            return (false, GlobalConstVars.AuthFreezeFailure, string.Empty, []);

        var userRoles = await userRoleRepo.GetListAsync(ur => ur.UserId == user.Id);
        if (userRoles.Count == 0)
            return (false, "账号未分配角色，请联系管理员", string.Empty, []);

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await roleRepo.GetListAsync(r => roleIds.Contains(r.Id));
        if (roles.Count == 0)
            return (false, "账号未分配角色，请联系管理员", string.Empty, []);

        var roleNameStr = string.Join(',', roles.Select(r => r.RoleCode));
        user.Roles = roles;
        var isAdmin = roles.Exists(r => string.Equals(r.RoleCode, AuthConstants.AdminRoleCode, StringComparison.OrdinalIgnoreCase));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.GivenName, user.RealName),
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        if (!string.IsNullOrEmpty(roleNameStr))
            claims.AddRange(roleNameStr.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

        if (isAdmin)
            claims.Add(new Claim(AuthConstants.ClaimIsAdmin, "true"));

        var token = JwtToken.BuildJwtToken([.. claims], jwtSigningSettings);

        return (true, GlobalConstVars.AuthSuccess, token, roles);
    }

    /// <summary>
    /// 写入平台登录访问记录
    /// </summary>
    private async Task LogLoginAsync(string userName, LoginRecordType type, string comments)
    {
        var log = new SysRecordLogin
        {
            UserName = userName,
            Os = RuntimeInformation.OSDescription,
            OperType = type,
            Comments = comments,
            LoginSource = LoginSource.平台登录.ToString(),
            CreateTime = DateTime.Now
        };

        if (httpContextAccessor.HttpContext is not null)
            log.Browser = httpContextAccessor.HttpContext.Request.Headers[UserAgentHeaderName];

        await loginRecordRepo.InsertAsync(log);
    }
}
