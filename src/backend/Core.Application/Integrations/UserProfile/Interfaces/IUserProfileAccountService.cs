namespace Core.Application;

public interface IUserProfileAccountService
{
    Task<CustomApiResponse<bool>> EditUserPasswordAsync(UserEditPasswordReq request);

    Task<CustomApiResponse> SendChangePhoneSmsAsync();

    Task<CustomApiResponse> SendChangePhoneSmsToNewAsync(UserSendChangePhoneNewSmsReq request);

    Task<CustomApiResponse> VerifyChangePhoneSmsAsync(UserVerifyChangePhoneReq request);

    Task<CustomApiResponse> EditUserPhoneAsync(UserEditPhoneReq request);
}
