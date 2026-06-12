namespace Core.Application;

public class SysUserService(IBaseRepo<SysUser> userRepo
    , IBaseRepo<SysRole> roleRepo
    , IBaseRepo<SysUserRole> userRoleRepo
    , IUnitOfWork unitOfWork
    , IMapper mapper) : ISysUserService
{
    const int SearchBriefMaxCount = 20;

    /// <summary>
    /// 分页查询用户列表（含角色；SysOrgId 组织机构能力预留，本接口暂不筛选与填充）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<List<SysUserResp>>> GetPageListAsync(SysUserPageQueryReq request)
    {
        var where = PredicateBuilder.True<SysUser>();

        if (request.RoleId.HasValue)
        {
            var roleUsers = await userRoleRepo.GetListAsync(p => p.RoleId == request.RoleId);
            if (roleUsers.Count == 0)
                where = PredicateBuilder.False<SysUser>();
            else
            {
                var ids = roleUsers.Select(a => a.UserId).ToList();
                where = where.And(p => ids.Contains(p.Id));
            }
        }

        if (request.Status.HasValue)
            where = where.And(p => p.Status == request.Status.Value);

        if (!string.IsNullOrEmpty(request.UserName))
            where = where.And(p => p.UserName.Contains(request.UserName));

        var list = await userRepo.GetPageAsync(where, p => p.Id, OrderByType.Desc, request.Page, request.PageSize);

        if (!list.Any())
            return CustomApiResponse<List<SysUserResp>>.Ok(GlobalConstVars.GetDataSuccess, [], list.TotalCount);

        var userIds = list.Select(a => a.Id).ToList();

        var userRoles = await userRoleRepo.GetListAsync(p => userIds.Contains(p.UserId));

        var roleDict = (userRoles.Count != 0
                ? await roleRepo.GetListAsync(p =>
                    userRoles.Select(r => r.RoleId).Distinct().ToList().Contains(p.Id))
                : [])
            .ToDictionary(p => p.Id, p => p);

        var byUserId = list.ToDictionary(p => p.Id);
        var roleIdsByUser = userRoles.ToLookup(ur => ur.UserId, ur => ur.RoleId);

        var userDtos = mapper.Map<List<SysUserResp>>(list);
        foreach (var item in userDtos)
        {
            if (!byUserId.TryGetValue(item.Id, out var u))
                continue;

            var roleIds = roleIdsByUser[u.Id];
            if (!roleIds.Any())
                continue;

            var rolesResp = new List<SysRoleResp>();
            foreach (var roleId in roleIds)
            {
                if (roleDict.TryGetValue(roleId, out var role))
                    rolesResp.Add(mapper.Map<SysRoleResp>(role));
            }

            item.Roles = rolesResp;
        }

        return CustomApiResponse<List<SysUserResp>>.Ok(GlobalConstVars.GetDataSuccess, userDtos, list.TotalCount);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> CreateAsync(SysUserReq request)
    {
        var existingUser = await userRepo.GetFirstAsync(p => p.UserName == request.UserName || p.Phone == request.Phone);

        if (existingUser is not null)
        {
            var msg = existingUser.UserName == request.UserName ? GlobalConstVars.RegUserNameExists : GlobalConstVars.RegPhoneExists;
            return CustomApiResponse.Fail(msg);
        }

        var entity = mapper.Map<SysUser>(request);
        entity.IdCardNumber = string.Empty;
        entity.SysOrgId = null; // 组织机构能力预留，开通暂不绑定机构
        entity.Status = UserStatus.正常;

        entity.Password = PasswordHelper.Hash(entity.Password);

        await unitOfWork.BeginTranAsync();
        try
        {
            var id = await userRepo.InsertAsync(entity);
            if (id <= 0)
            {
                await unitOfWork.RollbackTranAsync();
                return CustomApiResponse.Fail(GlobalConstVars.CreateFailure);
            }

            if (!string.IsNullOrEmpty(entity.RoleIds))
                await AddUserRolesAsync(entity.RoleIds, id);

            await unitOfWork.CommitTranAsync();
            return CustomApiResponse.Ok(GlobalConstVars.CreateSuccess);
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }
    }

    /// <summary>
    /// 根据主键获取用户（不含 Roles；管理端编辑请用列表行数据）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<SysUserResp>> GetByIdAsync(long id)
    {
        var model = await userRepo.GetByIdAsync(id);
        if (model is null)
            return CustomApiResponse<SysUserResp>.Fail(GlobalConstVars.DataIsNo);

        return CustomApiResponse<SysUserResp>.Ok(GlobalConstVars.GetDataSuccess, mapper.Map<SysUserResp>(model));
    }

    /// <summary>
    /// 编辑用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> EditAsync(long id, SysUserReq request)
    {
        var entity = await userRepo.GetByIdAsync(id);
        if (entity is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (await userRepo.IsAnyAsync(u => u.UserName == request.UserName && u.Id != id))
            return CustomApiResponse.Fail(GlobalConstVars.RegUserNameExists);

        if (await userRepo.IsAnyAsync(u => u.Phone == request.Phone && u.Id != id))
            return CustomApiResponse.Fail(GlobalConstVars.RegPhoneExists);

        entity.Sex = request.Sex;
        entity.UserName = request.UserName;
        entity.Phone = request.Phone;
        var passwordChanged = !string.IsNullOrEmpty(request.Password);
        entity.Password = passwordChanged ? PasswordHelper.Hash(request.Password) : entity.Password;

        await unitOfWork.BeginTranAsync();
        try
        {
            var result = await userRepo.EditAsync(entity);
            if (!result)
            {
                await unitOfWork.RollbackTranAsync();
                return CustomApiResponse.Fail(GlobalConstVars.EditFailure);
            }

            await userRoleRepo.DeleteAsync(p => p.UserId == entity.Id);
            if (!string.IsNullOrEmpty(request.RoleIds))
                await AddUserRolesAsync(request.RoleIds, entity.Id);

            await unitOfWork.CommitTranAsync();

            return CustomApiResponse.Ok(GlobalConstVars.EditSuccess);
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> DeleteAsync(long id)
    {
        var adminUser = await userRepo.GetFirstAsync(p => p.Id == id && p.UserName == AuthConstants.AdminUserName);
        if (adminUser is not null)
            return CustomApiResponse.Fail(GlobalConstVars.DeleteProhibitDelete);

        await unitOfWork.BeginTranAsync();
        try
        {
            await userRoleRepo.DeleteAsync(p => p.UserId == id);
            var result = await userRepo.DeleteAsync(id);
            if (!result)
            {
                await unitOfWork.RollbackTranAsync();
                return CustomApiResponse.Fail(GlobalConstVars.DeleteFailure);
            }

            await unitOfWork.CommitTranAsync();
            return CustomApiResponse.Ok(GlobalConstVars.DeleteSuccess);
        }
        catch
        {
            await unitOfWork.RollbackTranAsync();
            throw;
        }
    }

    /// <summary>
    /// 设置用户状态（锁定等）
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse> SetStatusAsync(long id, UserStatus status)
    {
        var entity = await userRepo.GetByIdAsync(id);
        if (entity is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (!Enum.IsDefined(status))
            return CustomApiResponse.Fail(GlobalConstVars.EnumValueInvalid);

        entity.Status = status;
        var result = await userRepo.EditAsync(entity);

        return result
            ? CustomApiResponse.Ok(GlobalConstVars.SetDataSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.SetDataFailure);
    }

    /// <summary>
    /// 按关键字搜索用户简要信息（账号、手机号、昵称，排除当前用户）
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public async Task<CustomApiResponse<List<UserBriefResp>>> SearchBriefAsync(long currentUserId, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return CustomApiResponse<List<UserBriefResp>>.Ok(GlobalConstVars.GetDataSuccess, []);

        var kw = keyword.Trim();
        var rows = await userRepo.GetListAsync(
            u => u.Id != currentUserId && (u.UserName.Contains(kw) || u.Phone.Contains(kw) || u.NickName.Contains(kw)),
            u => u.CreateTime,
            OrderByType.Desc);

        var list = rows.Take(SearchBriefMaxCount).Select(u => new UserBriefResp
        {
            Id = u.Id.ToString(),
            UserName = u.UserName,
            NickName = u.NickName,
            Phone = string.IsNullOrEmpty(u.Phone) || u.Phone.Length < 7 ? u.Phone : $"{u.Phone[..3]}****{u.Phone[^4..]}"
        }).ToList();

        return CustomApiResponse<List<UserBriefResp>>.Ok(GlobalConstVars.GetDataSuccess, list);
    }

    /// <summary>
    /// 按逗号分隔的角色 Id 写入用户角色关联（库表可多角色；当前产品约定一人一角色，前端仅传单个 id）
    /// </summary>
    private async Task AddUserRolesAsync(string roleIdsStr, long userId)
    {
        var strIds = roleIdsStr.Split(",");
        var ids = CommonHelper.StringToLongArray(strIds);
        if (ids.Length == 0) return;

        var userRoles = new List<SysUserRole>();
        foreach (var itemRoleId in ids)
        {
            userRoles.Add(new SysUserRole
            {
                RoleId = itemRoleId,
                UserId = userId,
            });
        }

        if (userRoles.Count > 0)
            await userRoleRepo.InsertRangeAsync(userRoles);
    }
}
