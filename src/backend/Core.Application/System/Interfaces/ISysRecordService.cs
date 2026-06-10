namespace Core.Application;

public interface ISysRecordService
{
    Task<CustomApiResponse<List<SysRecordLoginResp>>> GetLoginPageListAsync(SysRecordLoginPageQueryReq request);

    Task<CustomApiResponse> ClearLoginDataAsync();

    Task<CustomApiResponse<List<SysRecordNlogResp>>> GetNLogPageListAsync(SysRecordNlogPageQueryReq request);

    Task<CustomApiResponse> ClearNLogDataAsync();
}
