namespace Core.Application;

/// <summary>
/// 用户注册应用服务
/// </summary>
public class RegisterAuthService(
    IBaseRepo<SysUser> userRepo,
    IBaseRepo<SysUserRole> userRoleRepo,
    IBaseRepo<SysRole> roleRepo,
    ISmsCodeVerification smsCodeVerification,
    IUnitOfWork unitOfWork) : IRegisterAuthService
{
    /// <summary>
    /// 短信校验通过后注册并绑定默认角色
    /// </summary>
    public async Task<CustomApiResponse> RegisterAsync(SysUserRegReq request)
    {
        var existUserName = await userRepo.IsAnyAsync(p => p.UserName == request.UserName);
        if (existUserName)
            return CustomApiResponse.Fail(GlobalConstVars.RegUserNameExists);

        var existPhone = await userRepo.IsAnyAsync(p => p.Phone == request.Phone);
        if (existPhone)
            return CustomApiResponse.Fail(GlobalConstVars.RegPhoneExists);

        if (!smsCodeVerification.Verify(request.Phone, request.SmsCode))
            return CustomApiResponse.Fail(GlobalConstVars.RegSmsInvalid);

        var password = PasswordHelper.Hash(request.Password);

        var sysUser = new SysUser
        {
            UserName = request.UserName,
            Password = password,
            Phone = request.Phone,
            Sex = UserSexType.未知,
            UserType = UserType.注册用户,
            Status = UserStatus.正常
        };

        await unitOfWork.BeginTranAsync();
        try
        {
            var userId = await userRepo.InsertAsync(sysUser);
            if (userId <= 0)
            {
                await unitOfWork.RollbackTranAsync();
                return CustomApiResponse.Fail(GlobalConstVars.RegFailed);
            }

            var defaultRole = await roleRepo.GetFirstAsync(p => p.RoleCode == AuthConstants.RegisteredUserRoleCode);
            if (defaultRole is null)
            {
                await unitOfWork.RollbackTranAsync();
                return CustomApiResponse.Fail("系统未配置默认角色，请联系管理员");
            }

            await userRoleRepo.InsertAsync(new SysUserRole
            {
                UserId = userId,
                RoleId = defaultRole.Id
            });

            await unitOfWork.CommitTranAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTranAsync();
            var detail = ex.ToString();
            if (detail.Contains("ux_sys_user_user_name", StringComparison.OrdinalIgnoreCase))
                return CustomApiResponse.Fail(GlobalConstVars.RegUserNameExists);
            if (detail.Contains("ux_sys_user_phone", StringComparison.OrdinalIgnoreCase))
                return CustomApiResponse.Fail(GlobalConstVars.RegPhoneExists);

            throw;
        }

        return CustomApiResponse.Ok(GlobalConstVars.RegSuccess);
    }
}
