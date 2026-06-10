namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 用户个人资料管理
/// </summary>
/// <param name="userProfileService"></param>
[Description("用户个人资料管理")]
public class UserProfileController(IUserProfileService userProfileService) : AuthorizedControllerBase
{
    /// <summary>
    /// 头像 multipart 单文件大小上限（字节），须与 UserProfileService 内校验一致
    /// </summary>
    private const int AvatarUploadMaxBytes = 3145728;

    /// <summary>
    /// 获取用户详情数据
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Description("获取用户详情数据")]
    public Task<CustomApiResponse<SysUserResp>> GetUserInfo() =>
        userProfileService.GetUserInfoAsync();

    /// <summary>
    /// 根据用户权限获取菜单列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Description("根据用户权限获取菜单列表")]
    public Task<CustomApiResponse<List<SysMenuResp>>> GetMenus([FromQuery] string roleCode) =>
        userProfileService.GetMenusAsync(roleCode);

    /// <summary>
    /// 用户头像上传（multipart 字段 file；类型与大小与 OSS 暂存上传一致）
    /// </summary>
    /// <param name="file">图片文件</param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Description("用户头像上传")]
    [RequestSizeLimit(AvatarUploadMaxBytes)]
    public Task<CustomApiResponse<bool>> UploadAvatar(IFormFile file) =>
        userProfileService.UploadAvatarAsync(file);

    /// <summary>
    /// 编辑用户详情数据
    /// </summary>
    /// <param name="id">用户主键（须与当前登录用户一致）</param>
    /// <param name="request">编辑请求体</param>
    /// <returns></returns>
    [HttpPost("{id}")]
    [Description("编辑用户详情数据")]
    public Task<CustomApiResponse<bool>> EditUserInfo(long id, [FromBody] SysUserEditInfoReq request) =>
        userProfileService.EditUserInfoAsync(id, request);

    /// <summary>
    /// 用户修改密码
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Description("用户修改密码")]
    public Task<CustomApiResponse<bool>> EditUserPassword([FromBody] UserEditPasswordReq request) =>
        userProfileService.EditUserPasswordAsync(request);

    /// <summary>
    /// 用户更换手机号
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Description("用户更换手机号")]
    public Task<CustomApiResponse> EditUserPhone([FromBody] UserEditPhoneReq request) =>
        userProfileService.EditUserPhoneAsync(request);

    /// <summary>
    /// 向当前绑定手机号发送换绑校验短信
    /// </summary>
    [HttpPost]
    [Description("向当前绑定手机号发送换绑校验短信")]
    public Task<CustomApiResponse> SendChangePhoneSms() =>
        userProfileService.SendChangePhoneSmsAsync();

    /// <summary>
    /// 向换绑目标手机号发送短信验证码
    /// </summary>
    [HttpPost]
    [Description("向换绑目标手机号发送短信验证码")]
    public Task<CustomApiResponse> SendChangePhoneSmsToNew([FromBody] UserSendChangePhoneNewSmsReq request) =>
        userProfileService.SendChangePhoneSmsToNewAsync(request);

    /// <summary>
    /// 校验当前绑定手机号收到的换绑验证码
    /// </summary>
    [HttpPost]
    [Description("校验当前绑定手机号收到的换绑验证码")]
    public Task<CustomApiResponse> VerifyChangePhoneSms([FromBody] UserVerifyChangePhoneReq request) =>
        userProfileService.VerifyChangePhoneSmsAsync(request);
}
