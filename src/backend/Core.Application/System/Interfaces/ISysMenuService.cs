namespace Core.Application;

public interface ISysMenuService
{
    Task<CustomApiResponse<List<SysMenuResp>>> GetListAsync();

    Task<CustomApiResponse<List<SysMenuResp>>> GetTreeNodesAsync(long parentId);

    Task<CustomApiResponse<List<SysMenuResp>>> GetButtonsAsync(long parentMenuId);

    Task<CustomApiResponse<SysMenuResp>> GetByIdAsync(long id);

    Task<CustomApiResponse> CreateAsync(SysMenuReq request);

    Task<CustomApiResponse> EditAsync(long id, SysMenuReq request);

    Task<CustomApiResponse> DeleteAsync(long id);

    Task<CustomApiResponse> ImportButtonsAsync(long parentMenuId, List<SysMenuImportButtonItemReq> items);
}
