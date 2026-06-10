namespace Core.Application;

/// <summary>
/// 用户个人资料，对外统一入口（Controller 只注入本类）
/// </summary>
public class UserProfileService(
    IUserProfileInfoService userProfileInfoService,
    IUserProfileAccountService userProfileAccountService,
    IBaseRepo<SysRoleMenu> roleMenuRepo,
    IBaseRepo<SysRole> roleRepo,
    IBaseRepo<SysMenu> menuRepo,
    IMapper mapper) : IUserProfileService
{
    /// <summary>
    /// 获取当前登录用户详情
    /// </summary>
    public Task<CustomApiResponse<SysUserResp>> GetUserInfoAsync() =>
        userProfileInfoService.GetUserInfoAsync();

    /// <summary>
    /// 按角色编码加载可见目录菜单并组装为树（供前端动态路由），roleCode 必填
    /// </summary>
    public async Task<CustomApiResponse<List<SysMenuResp>>> GetMenusAsync(string roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            return CustomApiResponse<List<SysMenuResp>>.Fail(GlobalConstVars.DataParameterError);

        roleCode = roleCode.Trim().ToLowerInvariant();

        var role = await roleRepo.GetFirstAsync(p => p.RoleCode == roleCode);
        if (role is null)
            return CustomApiResponse<List<SysMenuResp>>.Ok(GlobalConstVars.GetDataSuccess, [], 0);

        var roleMenus = await roleMenuRepo.GetListAsync(p => p.RoleId == role.Id);
        var menuIdSet = roleMenus.Select(p => p.MenuId).ToHashSet();

        if (menuIdSet.Count == 0)
            return CustomApiResponse<List<SysMenuResp>>.Ok(GlobalConstVars.GetDataSuccess, [], 0);

        var allMenus = await menuRepo.GetListAsync(
            p => menuIdSet.Contains(p.Id) && p.Status == 1 && p.MenuType == 0,
            p => p.Sort,
            OrderByType.Asc);

        var flat = mapper.Map<List<SysMenuResp>>(allMenus);

        List<SysMenuResp> tree(long? parentId) =>
            [.. flat
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Sort)
                .Select(node =>
                {
                    node.Children = tree(node.Id);
                    return node;
                })];

        return CustomApiResponse<List<SysMenuResp>>.Ok(GlobalConstVars.GetDataSuccess, tree(null), allMenus.Count);
    }

    /// <summary>
    /// 用户头像上传
    /// </summary>
    public Task<CustomApiResponse<bool>> UploadAvatarAsync(IFormFile file) =>
        userProfileInfoService.UploadAvatarAsync(file);

    /// <summary>
    /// 编辑用户资料
    /// </summary>
    public Task<CustomApiResponse<bool>> EditUserInfoAsync(long id, SysUserEditInfoReq request) =>
        userProfileInfoService.EditUserInfoAsync(id, request);

    /// <summary>
    /// 修改登录密码
    /// </summary>
    public Task<CustomApiResponse<bool>> EditUserPasswordAsync(UserEditPasswordReq request) =>
        userProfileAccountService.EditUserPasswordAsync(request);

    /// <summary>
    /// 向当前绑定手机号发送换绑验证码
    /// </summary>
    public Task<CustomApiResponse> SendChangePhoneSmsAsync() =>
        userProfileAccountService.SendChangePhoneSmsAsync();

    /// <summary>
    /// 向换绑目标手机号发送短信验证码
    /// </summary>
    public Task<CustomApiResponse> SendChangePhoneSmsToNewAsync(UserSendChangePhoneNewSmsReq request) =>
        userProfileAccountService.SendChangePhoneSmsToNewAsync(request);

    /// <summary>
    /// 校验当前绑定手机号收到的换绑验证码
    /// </summary>
    public Task<CustomApiResponse> VerifyChangePhoneSmsAsync(UserVerifyChangePhoneReq request) =>
        userProfileAccountService.VerifyChangePhoneSmsAsync(request);

    /// <summary>
    /// 更换用户绑定手机号
    /// </summary>
    public Task<CustomApiResponse> EditUserPhoneAsync(UserEditPhoneReq request) =>
        userProfileAccountService.EditUserPhoneAsync(request);
}
