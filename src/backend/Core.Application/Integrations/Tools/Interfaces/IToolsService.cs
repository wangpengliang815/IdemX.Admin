namespace Core.Application;

public interface IToolsService
{
    Task<CustomApiResponse<List<SysAreaItemResp>>> GetAreaByCodeAsync(SysAreaGetByCodeReq request);

    Task<CustomApiResponse<List<SysAreaTreeNodeResp>>> GetAreaTreeAsync();

    CustomApiResponse<Dictionary<string, List<EnumOptionItemResp>>> GetEnum();
}
