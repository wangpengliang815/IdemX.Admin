namespace Core.DataAccess;

/// <summary>
/// 工作单元实现（异步版本）
/// </summary>
/// <param name="dbClient">SqlSugar 数据库客户端（由 .NET Core IOC 注册）</param>
public class UnitOfWork(SqlSugarScope dbClient) : IUnitOfWork
{
    /// <summary>
    /// 获取数据库客户端
    /// </summary>
    public SqlSugarScope GetDbClient() => dbClient;

    /// <summary>
    /// 开启事务
    /// </summary>
    public Task BeginTranAsync()
    {
        dbClient.BeginTran();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public Task CommitTranAsync()
    {
        try
        {
            dbClient.CommitTran();
            return Task.CompletedTask;
        }
        catch (Exception)
        {
            dbClient.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public Task RollbackTranAsync()
    {
        dbClient.RollbackTran();
        return Task.CompletedTask;
    }
}

