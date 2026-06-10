namespace Core.Application;

public interface IAuthService
{
    Task<CustomApiResponse<string>> LoginByPasswordAsync(LoginByPasswordReq request);

    Task<CustomApiResponse<string>> LoginByPhoneAsync(LoginByPhoneReq request);

    Task<CustomApiResponse> RegisterAsync(SysUserRegReq request);

    Task<CustomApiResponse<bool>> CheckPhoneExistsAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> SendSmsCodeAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> SendForgotPasswordSmsAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneReq request);

    Task<CustomApiResponse> LogoutAsync();
}
