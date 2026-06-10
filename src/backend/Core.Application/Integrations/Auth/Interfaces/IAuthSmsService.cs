namespace Core.Application;

public interface IAuthSmsService
{
    Task<CustomApiResponse<bool>> CheckPhoneExistsAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> SendSmsCodeAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> SendForgotPasswordSmsAsync(SmsSendCodeReq request);

    Task<CustomApiResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneReq request);
}
