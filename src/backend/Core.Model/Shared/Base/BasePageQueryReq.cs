namespace Core.Model.Shared;

/// <summary>
/// 分页查询基础 DTO
/// </summary>
public abstract class BasePageQueryReq
{
    /// <summary>
    /// 页码（从 1 开始）
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; } = 30;
}
