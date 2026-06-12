namespace Core.Application;

public interface ISysUserService
{
    Task<CustomApiResponse<List<SysUserResp>>> GetPageListAsync(SysUserPageQueryReq request);

    Task<CustomApiResponse<List<UserBriefResp>>> SearchBriefAsync(long currentUserId, string keyword);

    Task<CustomApiResponse> CreateAsync(SysUserReq request);

    Task<CustomApiResponse<SysUserResp>> GetByIdAsync(long id);

    Task<CustomApiResponse> EditAsync(long id, SysUserReq request);

    Task<CustomApiResponse> DeleteAsync(long id);

    Task<CustomApiResponse> SetStatusAsync(long id, UserStatus status);
}
