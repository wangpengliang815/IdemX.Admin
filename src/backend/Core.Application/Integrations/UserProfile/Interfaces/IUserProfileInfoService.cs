namespace Core.Application;

public interface IUserProfileInfoService
{
    Task<CustomApiResponse<SysUserResp>> GetUserInfoAsync();

    Task<CustomApiResponse<bool>> UploadAvatarAsync(IFormFile file);

    Task<CustomApiResponse<bool>> EditUserInfoAsync(long id, SysUserEditInfoReq request);
}
