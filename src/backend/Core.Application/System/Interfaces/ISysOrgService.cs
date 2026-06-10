namespace Core.Application;

public interface ISysOrgService
{
    Task<CustomApiResponse<List<SysOrgResp>>> GetListAsync();

    Task<CustomApiResponse<List<SysOrgResp>>> GetTreeNodesAsync(long parentId);

    Task<CustomApiResponse> CreateAsync(SysOrgReq request);

    Task<CustomApiResponse<SysOrgResp>> GetByIdAsync(long id);

    Task<CustomApiResponse> EditAsync(long id, SysOrgReq request);

    Task<CustomApiResponse> DeleteAsync(long id);
}
