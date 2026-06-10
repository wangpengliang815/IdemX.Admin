namespace Core.Infrastructure.NetCore;

/// <summary>
/// SqlSugar 启动服务（使用 .NET Core 自带 IOC）
/// </summary>
public static class SqlSugarSetup
{
    public static void AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var dbTypeName = configuration["ConnectionStrings:DbType"];
        var dbType = string.Equals(dbTypeName, "SqlServer", StringComparison.OrdinalIgnoreCase)
            ? DbType.SqlServer
            : DbType.PostgreSQL;

        var connectionString = configuration.GetConnectionString("SqlConnection");

        services.AddSingleton<SqlSugarScope>(serviceProvider =>
        {
            var sqlSugarScope = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = connectionString,
                DbType = dbType,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                MoreSettings = new ConnMoreSettings
                {
                    IsNoReadXmlDescription = true,
                },
            }, db =>
            {
                // 实体属性名转数据库字段名（驼峰转下划线并小写）
                db.CurrentConnectionConfig.ConfigureExternalServices = new ConfigureExternalServices()
                {
                    EntityService = (property, column) =>
                    {
                        column.DbColumnName = UtilMethods.ToUnderLine(property.Name).ToLower();
                    }
                };

                // 全局查询过滤器：自动过滤软删除数据
                db.QueryFilter.AddTableFilter<IDeleted>(entity => entity.IsDeleted == false);

                // 开发环境打印SQL日志
                db.Aop.OnLogExecuting = (sql, p) =>
                {
                    //Console.WriteLine($"SQL: {sql}");
                };

                // SQL执行错误时记录日志
                db.Aop.OnError = (exp) =>
                {
                    NLogUtil.Error(LogType.DataBase, "SQL错误", $"{exp.Sql}", exp);
                };

                // 审计字段自动填充 AOP（通过闭包传递 serviceProvider，在 AOP 中可以获取 IOC 对象）
                db.Aop.DataExecuting = (oldValue, entityInfo) =>
                {
                    ConfigureAuditAop(entityInfo, serviceProvider);
                };
            });

            return sqlSugarScope;
        });

        // 同时注册 ISqlSugarClient（如果需要注入接口类型）
        services.AddSingleton<ISqlSugarClient>(serviceProvider =>
            serviceProvider.GetRequiredService<SqlSugarScope>());
    }

    /// <summary>
    /// 配置审计字段自动填充
    /// </summary>
    /// <param name="entityInfo">实体信息</param>
    /// <param name="serviceProvider">服务提供者（从 SqlSugarScope 创建时的闭包传入）</param>
    private static void ConfigureAuditAop(DataFilterModel entityInfo, IServiceProvider serviceProvider)
    {
        // 只处理 BaseEntity 及其子类
        if (entityInfo.EntityValue is not BaseEntity entity)
            return;

        // 通过 HttpContextAccessor 获取当前请求的服务（AOP 中可以获取 IOC 对象）
        // 参考文档：https://www.donet5.com/home/doc?masterId=1&typeId=2247
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        var requestServices = httpContextAccessor?.HttpContext?.RequestServices;

        // 插入时自动填充
        if (entityInfo.OperationType == DataFilterType.InsertByObject)
        {
            switch (entityInfo.PropertyName)
            {
                case nameof(BaseEntity.Id) when entity.Id == 0:
                    // 优先从请求作用域获取，如果没有则从根容器获取
                    var idGen = requestServices?.GetService<IIdGenerator>() ?? serviceProvider.GetService<IIdGenerator>();
                    if (idGen != null)
                    {
                        var newId = idGen.NextId();
                        entityInfo.SetValue(newId);
                    }
                    break;

                case nameof(BaseEntity.CreateTime) when entity.CreateTime == default:
                    entityInfo.SetValue(DateTime.Now);
                    break;

                case nameof(BaseEntity.CreateBy) when entity.CreateBy == null:
                    var operatorContext = requestServices?.GetService<IOperatorContext>() ?? serviceProvider.GetService<IOperatorContext>();
                    var operatorId = operatorContext?.GetCurrentOperatorId() ?? OperatorConstants.System;
                    entityInfo.SetValue(operatorId);
                    break;

                case nameof(BaseEntity.CreateByUsername) when entity.CreateByUsername == null:
                    var createUsernameCtx = requestServices?.GetService<IOperatorContext>() ?? serviceProvider.GetService<IOperatorContext>();
                    entityInfo.SetValue(createUsernameCtx?.GetCurrentOperatorUsername());
                    break;
            }
        }
        // 更新时自动填充
        else if (entityInfo.OperationType == DataFilterType.UpdateByObject)
        {
            switch (entityInfo.PropertyName)
            {
                case nameof(BaseEntity.UpdateTime):
                    entityInfo.SetValue(DateTime.Now);
                    break;

                case nameof(BaseEntity.UpdateBy) when entity.UpdateBy == null:
                    var updateOperatorContext = requestServices?.GetService<IOperatorContext>() ?? serviceProvider.GetService<IOperatorContext>();
                    var updateOperatorId = updateOperatorContext?.GetCurrentOperatorId() ?? OperatorConstants.System;
                    entityInfo.SetValue(updateOperatorId);
                    break;

                case nameof(BaseEntity.UpdateByUsername) when entity.UpdateByUsername == null:
                    var updateUsernameCtx = requestServices?.GetService<IOperatorContext>() ?? serviceProvider.GetService<IOperatorContext>();
                    entityInfo.SetValue(updateUsernameCtx?.GetCurrentOperatorUsername());
                    break;

                case nameof(BaseEntity.DeleteTime) when entity.IsDeleted && entity.DeleteTime == null:
                    entityInfo.SetValue(DateTime.Now);
                    break;

                case nameof(BaseEntity.DeleteBy) when entity.IsDeleted && entity.DeleteBy == null:
                    var deleteOperatorContext = requestServices?.GetService<IOperatorContext>() ?? serviceProvider.GetService<IOperatorContext>();
                    var deleteOperatorId = deleteOperatorContext?.GetCurrentOperatorId() ?? OperatorConstants.System;
                    entityInfo.SetValue(deleteOperatorId);
                    break;

                case nameof(BaseEntity.DeleteByUsername) when entity.IsDeleted && entity.DeleteByUsername == null:
                    var deleteUsernameCtx = requestServices?.GetService<IOperatorContext>() ?? serviceProvider.GetService<IOperatorContext>();
                    entityInfo.SetValue(deleteUsernameCtx?.GetCurrentOperatorUsername());
                    break;
            }
        }
    }
}
