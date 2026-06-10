namespace Core.Application;

public interface IUserProfileService
{
    Task<CustomApiResponse<SysUserResp>> GetUserInfoAsync();

    Task<CustomApiResponse<List<SysMenuResp>>> GetMenusAsync(string roleCode);

    Task<CustomApiResponse<bool>> UploadAvatarAsync(IFormFile file);

    Task<CustomApiResponse<bool>> EditUserInfoAsync(long id, SysUserEditInfoReq request);

    Task<CustomApiResponse<bool>> EditUserPasswordAsync(UserEditPasswordReq request);

    Task<CustomApiResponse> SendChangePhoneSmsAsync();

    Task<CustomApiResponse> SendChangePhoneSmsToNewAsync(UserSendChangePhoneNewSmsReq request);

    Task<CustomApiResponse> VerifyChangePhoneSmsAsync(UserVerifyChangePhoneReq request);

    Task<CustomApiResponse> EditUserPhoneAsync(UserEditPhoneReq request);
}
