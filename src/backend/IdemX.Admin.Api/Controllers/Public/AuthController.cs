namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 用户授权登录（公开接口，无需授权）
/// </summary>
[Route("api/[controller]/[action]")]
[RequiredError]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// 账号密码登录
    /// </summary>
    [HttpPost]
    public Task<CustomApiResponse<string>> LoginByPassword([FromBody] LoginByPasswordReq request) =>
        authService.LoginByPasswordAsync(request);

    /// <summary>
    /// 手机号登录
    /// </summary>
    [HttpPost]
    public Task<CustomApiResponse<string>> LoginByPhone([FromBody] LoginByPhoneReq request) =>
        authService.LoginByPhoneAsync(request);

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost]
    public Task<CustomApiResponse> Register([FromBody] SysUserRegReq request) =>
        authService.RegisterAsync(request);

    /// <summary>
    /// 检查手机号是否已存在
    /// </summary>
    [HttpPost]
    public Task<CustomApiResponse<bool>> CheckPhoneExists([FromBody] SmsSendCodeReq request) =>
        authService.CheckPhoneExistsAsync(request);

    /// <summary>
    /// 发送验证码
    /// </summary>
    [HttpPost]
    public Task<CustomApiResponse> SendSmsCode([FromBody] SmsSendCodeReq request) =>
        authService.SendSmsCodeAsync(request);

    /// <summary>
    /// 忘记密码：向已注册手机号发送验证码
    /// </summary>
    [HttpPost]
    public Task<CustomApiResponse> SendForgotPasswordSms([FromBody] SmsSendCodeReq request) =>
        authService.SendForgotPasswordSmsAsync(request);

    /// <summary>
    /// 通过手机短信验证码重置密码
    /// </summary>
    [HttpPost]
    public Task<CustomApiResponse> ResetPasswordByPhone([FromBody] ResetPasswordByPhoneReq request) =>
        authService.ResetPasswordByPhoneAsync(request);

    /// <summary>
    /// 退出登录
    /// </summary>
    [HttpPost]
    [Authorize]
    public Task<CustomApiResponse> Logout() => authService.LogoutAsync();
}
