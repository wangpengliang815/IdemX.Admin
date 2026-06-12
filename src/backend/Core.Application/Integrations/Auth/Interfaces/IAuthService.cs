namespace Core.Application;

public interface IAuthService
{
    Task<CustomApiResponse<string>> LoginByPasswordAsync(LoginByPasswordReq request);

    Task<CustomApiResponse> SendForgotPasswordSmsAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneReq request);

    Task<CustomApiResponse> LogoutAsync();
}
