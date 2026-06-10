namespace Core.Application;

public interface ISysRoleService
{
    Task<CustomApiResponse<List<SysRoleResp>>> GetListAsync();

    Task<CustomApiResponse<List<SysRoleResp>>> GetPageListAsync(SysRolePageQueryReq request);

    Task<CustomApiResponse> CreateAsync(SysRoleReq request);

    Task<CustomApiResponse<SysRoleResp>> GetByIdAsync(long id);

    Task<CustomApiResponse> EditAsync(long id, SysRoleReq request);

    Task<CustomApiResponse> DeleteAsync(long id);

    Task<CustomApiResponse<SysRoleMenuMapResp>> GetRoleMenuMapAsync(long id);

    Task<CustomApiResponse> SetRoleMenuMapAsync(long roleId, List<string> menuIds);
}
