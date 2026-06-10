namespace Core.DataAccess;

public class BaseRepo<T>(IUnitOfWork unitOfWork) : IBaseRepo<T> where T : BaseEntity, new()
{
    /// <summary>
    /// 工作单元（用于事务管理）
    /// </summary>
    public IUnitOfWork UnitOfWork { get; } = unitOfWork;

    /// <summary>
    /// 数据库客户端（用于复杂查询，如关闭全局过滤器）
    /// </summary>
    protected readonly SqlSugarScope DbClient = unitOfWork.GetDbClient();

    /// <summary>
    /// 按主键查一条，无记录返回 null（SqlSugar In + FirstAsync）
    /// </summary>
    public async Task<T> GetByIdAsync(long id)
    {
        return await DbClient.Queryable<T>().In(id).FirstAsync();
    }

    /// <summary>
    /// 根据主键批量查询
    /// </summary>  
    public async Task<List<T>> GetByIdsAsync(IEnumerable<long> ids)
    {
        return await DbClient.Queryable<T>().In(ids.ToList()).ToListAsync();
    }

    /// <summary>
    /// 按条件取一条；无匹配行返回 null。未传 orderBy 时走 SqlSugar FirstAsync；传 orderBy 时在命中集合上排序后取一条。
    /// </summary>
    public async Task<T> GetFirstAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, object>> orderBy = null,
        OrderByType orderByType = OrderByType.Desc)
    {
        if (orderBy is null)
            return await DbClient.Queryable<T>().FirstAsync(predicate);

        return await DbClient.Queryable<T>()
            .Where(predicate)
            .OrderByIF(true, orderBy, orderByType)
            .Take(1)
            .FirstAsync();
    }

    /// <summary>
    /// 根据条件判断是否存在
    /// </summary>
    public async Task<bool> IsAnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbClient.Queryable<T>().AnyAsync(predicate);
    }

    /// <summary>
    /// 根据条件查询列表
    /// </summary>
    public async Task<List<T>> GetListAsync(
        Expression<Func<T, bool>> predicate = null,
        Expression<Func<T, object>> orderBy = null,
        OrderByType orderByType = OrderByType.Desc)
    {
        return await DbClient.Queryable<T>()
            .WhereIF(predicate != null, predicate)
            .OrderByIF(orderBy != null, orderBy, orderByType)
            .ToListAsync();
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public async Task<IPageList<T>> GetPageAsync(
        Expression<Func<T, bool>> predicate = null,
        Expression<Func<T, object>> orderBy = null,
        OrderByType orderByType = OrderByType.Desc,
        int pageIndex = 1,
        int pageSize = 20)
    {
        RefAsync<int> totalCount = 0;

        var list = await DbClient.Queryable<T>()
            .WhereIF(predicate != null, predicate)
            .OrderByIF(orderBy != null, orderBy, orderByType)
            .ToPageListAsync(pageIndex, pageSize, totalCount);

        return new PageList<T>(list, pageIndex, pageSize, totalCount);
    }

    /// <summary>
    /// 插入单条（返回 ID）
    /// </summary>
    /// <remarks>
    /// Id 由 SqlSugar AOP 自动生成（雪花ID），无需手动赋值
    /// </remarks>
    public async Task<long> InsertAsync(T entity)
    {
        var result = await DbClient.Insertable(entity).ExecuteCommandAsync();
        return result > 0 ? entity.Id : 0;
    }

    /// <summary>
    /// 批量插入（返回影响行数）
    /// </summary>
    /// <remarks>
    /// Id 由 SqlSugar AOP 自动生成（雪花ID），无需手动赋值
    /// </remarks>
    public async Task<int> InsertRangeAsync(List<T> entities)
    {
        if (entities == null || entities.Count == 0)
            return 0;

        return await DbClient.Insertable(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新单条
    /// </summary>
    public async Task<bool> EditAsync(T entity)
    {
        return await DbClient.Updateable(entity).ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 批量更新
    /// </summary>
    public async Task<bool> EditRangeAsync(List<T> entities)
    {
        if (entities == null || entities.Count == 0)
            return false;

        return await DbClient.Updateable(entities).ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 根据 ID 物理删除
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        return await DbClient.Deleteable<T>().In(id).ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 根据条件物理删除
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbClient.Deleteable(predicate).ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 批量物理删除
    /// </summary>
    public async Task<bool> DeleteRangeAsync(IEnumerable<long> ids)
    {
        var idList = ids?.ToList();
        if (idList == null || idList.Count == 0)
            return false;

        return await DbClient.Deleteable<T>().In(idList).ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 软删除（更新 IsDeleted/DeleteTime/DeleteBy）
    /// </summary>
    public async Task<bool> LogicDeleteAsync(T entity)
    {
        return await DbClient.Updateable(entity)
            .UpdateColumns(e => new { e.IsDeleted, e.DeleteTime, e.DeleteBy })
            .ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 批量软删除
    /// </summary>
    public async Task<bool> LogicDeleteRangeAsync(List<T> entities)
    {
        if (entities == null || entities.Count == 0)
            return false;

        return await DbClient.Updateable(entities)
            .UpdateColumns(e => new { e.IsDeleted, e.DeleteTime, e.DeleteBy })
            .ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 按条件批量软删除
    /// </summary>
    /// <param name="predicate">软删除条件</param>
    public async Task<bool> LogicDeleteRangeAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            return false;

        return await DbClient.Updateable<T>()
            .SetColumns(e => e.IsDeleted == true)
            .Where(predicate)
            .ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 执行原生 SQL 命令（如递归 CTE 等复杂场景）
    /// </summary>
    public async Task<bool> ExecuteCommandAsync(string sql, SugarParameter[] parameters = null)
    {
        var affectedRows = await DbClient.Ado.ExecuteCommandAsync(sql, parameters);
        return affectedRows > 0;
    }
}

