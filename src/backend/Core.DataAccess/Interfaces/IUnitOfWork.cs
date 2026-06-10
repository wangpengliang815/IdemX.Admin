namespace Core.DataAccess;

/// <summary>
/// 新工作单元接口（异步版本）
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 获取数据库客户端
    /// </summary>
    SqlSugarScope GetDbClient();

    /// <summary>
    /// 开启事务
    /// </summary>
    Task BeginTranAsync();

    /// <summary>
    /// 提交事务
    /// </summary>
    Task CommitTranAsync();

    /// <summary>
    /// 回滚事务
    /// </summary>
    Task RollbackTranAsync();
}

