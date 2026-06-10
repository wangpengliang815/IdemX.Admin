namespace Core.Application;

/// <summary>
/// 认证门面，对外统一入口
/// </summary>
public class AuthService(
    ILoginAuthService loginAuthService,
    IRegisterAuthService registerAuthService,
    IAuthSmsService authSmsService) : IAuthService
{
    /// <summary>
    /// 用户名密码登录
    /// </summary>
    public Task<CustomApiResponse<string>> LoginByPasswordAsync(LoginByPasswordReq request) =>
        loginAuthService.LoginByPasswordAsync(request);

    /// <summary>
    /// 手机号短信登录
    /// </summary>
    public Task<CustomApiResponse<string>> LoginByPhoneAsync(LoginByPhoneReq request) =>
        loginAuthService.LoginByPhoneAsync(request);

    /// <summary>
    /// 用户注册
    /// </summary>
    public Task<CustomApiResponse> RegisterAsync(SysUserRegReq request) =>
        registerAuthService.RegisterAsync(request);

    /// <summary>
    /// 检查手机号是否已注册
    /// </summary>
    public Task<CustomApiResponse<bool>> CheckPhoneExistsAsync(SmsSendCodeReq request) =>
        authSmsService.CheckPhoneExistsAsync(request);

    /// <summary>
    /// 发送登录或注册短信验证码
    /// </summary>
    public Task<CustomApiResponse> SendSmsCodeAsync(SmsSendCodeReq request) =>
        authSmsService.SendSmsCodeAsync(request);

    /// <summary>
    /// 发送找回密码短信验证码
    /// </summary>
    public Task<CustomApiResponse> SendForgotPasswordSmsAsync(SmsSendCodeReq request) =>
        authSmsService.SendForgotPasswordSmsAsync(request);

    /// <summary>
    /// 手机短信重置密码
    /// </summary>
    public Task<CustomApiResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneReq request) =>
        authSmsService.ResetPasswordByPhoneAsync(request);

    /// <summary>
    /// 退出登录
    /// </summary>
    public Task<CustomApiResponse> LogoutAsync() =>
        loginAuthService.LogoutAsync();
}
