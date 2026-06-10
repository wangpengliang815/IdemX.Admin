namespace Core.Application;

/// <summary>
/// 当前用户资料、头像与角色切换
/// </summary>
public class UserProfileInfoService(
    IBaseRepo<SysUser> userRepo,
    IBaseRepo<SysUserRole> userRoleRepo,
    IBaseRepo<SysRole> roleRepo,
    IBaseRepo<SysOrg> orgRepo,
    IOssProvider ossProvider,
    IHttpContextUser contextUser,
    IMapper mapper) : IUserProfileInfoService
{
    /// <summary>
    /// 获取当前登录用户详情（角色列表、机构名；并写入是否管理员标记）
    /// </summary>
    public async Task<CustomApiResponse<SysUserResp>> GetUserInfoAsync()
    {
        var user = await userRepo.GetByIdAsync(contextUser.Id);
        if (user is null)
            return CustomApiResponse<SysUserResp>.Fail(GlobalConstVars.DataIsNo);

        var roles = await userRoleRepo.GetListAsync(p => p.UserId == user.Id);
        if (roles.Count != 0)
        {
            var roleIds = roles.Select(p => p.RoleId).ToList();
            user.Roles = await roleRepo.GetListAsync(p => roleIds.Contains(p.Id));
        }

        if (user.SysOrgId is not null)
        {
            var org = await orgRepo.GetByIdAsync(user.SysOrgId.Value);
            if (org is not null)
                user.SysOrgName = org.Name;
        }

        var userDto = mapper.Map<SysUserResp>(user);
        userDto.IsAdmin = contextUser.IsAdmin;

        return CustomApiResponse<SysUserResp>.Ok(GlobalConstVars.GetDataSuccess, userDto);
    }

    /// <summary>
    /// 上传当前用户头像：表单图片直传 OSS（键 avatars/用户Id + 扩展名），类型与大小规则与 OSS 暂存上传一致
    /// </summary>
    public async Task<CustomApiResponse<bool>> UploadAvatarAsync(IFormFile file)
    {
        var userEntity = await userRepo.GetByIdAsync(contextUser.Id);
        if (userEntity is null)
            return CustomApiResponse<bool>.Fail(GlobalConstVars.DataIsNo);

        if (file is null || file.Length == 0)
            return CustomApiResponse<bool>.Fail(GlobalConstVars.OssTempImageUploadEmpty);

        if (file.Length > 3145728)
            return CustomApiResponse<bool>.Fail(GlobalConstVars.OssTempImageUploadTooLarge);

        if (!CommonHelper.TryGetOssTempImageUploadMeta(file.ContentType, out var extension, out var contentType))
            return CustomApiResponse<bool>.Fail(GlobalConstVars.OssTempImageUploadTypeInvalid);

        var objectKey = $"avatars/{userEntity.Id}{extension}";

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var oldUrl = userEntity.Avatar;
        var url = ossProvider.PutObject(ms, objectKey, contentType);
        if (!string.IsNullOrWhiteSpace(oldUrl))
        {
            var oldKey = ossProvider.PublicUrlToObjectKey(oldUrl);
            var prefix = $"avatars/{userEntity.Id}.";
            if (oldKey.Length > 0
                && !string.Equals(oldKey, objectKey, StringComparison.Ordinal)
                && oldKey.StartsWith(prefix, StringComparison.Ordinal))
                ossProvider.DeleteObject(oldKey);
        }

        userEntity.Avatar = url;
        var result = await userRepo.EditAsync(userEntity);
        return result
            ? CustomApiResponse<bool>.Ok(GlobalConstVars.UploadAvatarSuccess, true)
            : CustomApiResponse<bool>.Fail(GlobalConstVars.UploadAvatarFailure, false);
    }

    /// <summary>
    /// 编辑用户基本资料（昵称、性别、邮箱、微信号等）
    /// </summary>
    public async Task<CustomApiResponse<bool>> EditUserInfoAsync(long id, SysUserEditInfoReq request)
    {
        var userEntity = await userRepo.GetByIdAsync(id);
        if (userEntity is null)
            return CustomApiResponse<bool>.Fail(GlobalConstVars.DataIsNo);

        if (userEntity.Id != contextUser.Id)
            return CustomApiResponse<bool>.Fail(GlobalConstVars.DataAccessDenied);

        userEntity.NickName = request.NickName;
        userEntity.Sex = (UserSexType)request.Sex;
        userEntity.Email = request.Email;
        userEntity.WechatNo = request.WechatNo;

        var result = await userRepo.EditAsync(userEntity);
        return result
            ? CustomApiResponse<bool>.Ok(GlobalConstVars.EditSuccess)
            : CustomApiResponse<bool>.Fail(GlobalConstVars.EditFailure);
    }
}
