namespace Core.Application;

public interface IAuthSmsService
{
    Task<CustomApiResponse> SendForgotPasswordSmsAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneReq request);
}
