namespace Core.DataAccess;

/// <summary>
/// 泛型仓储，封装 SqlSugar 常用读写
/// </summary>
/// <remarks>
/// 主键：GetByIdAsync。条件单条：GetFirstAsync（可选 orderBy；无匹配行返回 null）。
/// </remarks>
public interface IBaseRepo<T> where T : BaseEntity, new()
{
    /// <summary>
    /// 按主键查一条，无记录返回 null（SqlSugar In + FirstAsync）
    /// </summary>
    Task<T> GetByIdAsync(long id);

    /// <summary>
    /// 根据主键批量查询
    /// </summary>
    Task<List<T>> GetByIdsAsync(IEnumerable<long> ids);

    /// <summary>
    /// 按条件取一条；无匹配行返回 null。未传 orderBy 时走 SqlSugar FirstAsync；传 orderBy 时在命中集合上排序后取一条。
    /// </summary>
    Task<T> GetFirstAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, object>> orderBy = null,
        OrderByType orderByType = OrderByType.Desc);

    /// <summary>
    /// 根据条件判断是否存在
    /// </summary>
    /// <param name="predicate">查询条件</param>
    Task<bool> IsAnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 根据条件查询列表
    /// </summary>
    /// <param name="predicate">查询条件，null 则查全部</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="orderByType">排序方式，默认倒序</param>
    Task<List<T>> GetListAsync(
        Expression<Func<T, bool>> predicate = null,
        Expression<Func<T, object>> orderBy = null,
        OrderByType orderByType = OrderByType.Desc);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="predicate">查询条件，null 则查全部</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="orderByType">排序方式，默认倒序</param>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页条数</param>
    Task<IPageList<T>> GetPageAsync(
        Expression<Func<T, bool>> predicate = null,
        Expression<Func<T, object>> orderBy = null,
        OrderByType orderByType = OrderByType.Desc,
        int pageIndex = 1,
        int pageSize = 20);

    /// <summary>
    /// 插入单条（返回 ID）
    /// </summary>
    /// <remarks>Id 由 SqlSugar AOP 自动生成，无需手动赋值</remarks>
    Task<long> InsertAsync(T entity);

    /// <summary>
    /// 批量插入（返回影响行数）
    /// </summary>
    /// <remarks>Id 由 SqlSugar AOP 自动生成，无需手动赋值</remarks>
    Task<int> InsertRangeAsync(List<T> entities);

    /// <summary>
    /// 更新单条
    /// </summary>
    Task<bool> EditAsync(T entity);

    /// <summary>
    /// 批量更新
    /// </summary>
    Task<bool> EditRangeAsync(List<T> entities);

    /// <summary>
    /// 根据 ID 物理删除
    /// </summary>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// 根据条件物理删除
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 批量物理删除
    /// </summary>
    Task<bool> DeleteRangeAsync(IEnumerable<long> ids);

    /// <summary>
    /// 软删除（更新 IsDeleted/DeleteTime/DeleteBy）
    /// </summary>
    /// <remarks>调用前需在 Service 层填充 DeleteTime/DeleteBy</remarks>
    Task<bool> LogicDeleteAsync(T entity);

    /// <summary>
    /// 批量软删除
    /// </summary>
    /// <remarks>调用前需在 Service 层填充 DeleteTime/DeleteBy</remarks>
    Task<bool> LogicDeleteRangeAsync(List<T> entities);

    /// <summary>
    /// 按条件批量软删除
    /// </summary>
    /// <param name="predicate">软删除条件</param>
    Task<bool> LogicDeleteRangeAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 执行原生 SQL 命令（如递归 CTE 等复杂场景）
    /// </summary>
    Task<bool> ExecuteCommandAsync(string sql, SugarParameter[] parameters = null);
}

