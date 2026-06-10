namespace Core.Infrastructure.NetCore;

public class AutofacModuleRegister : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        Assembly assemblyApplication;
        Assembly assemblyDataAccess;

        try
        {
            assemblyApplication = Assembly.Load("Core.Application");
            assemblyDataAccess = Assembly.Load("Core.DataAccess");
        }
        catch (Exception ex)
        {
            var errorMsg = $"加载程序集失败：{ex.Message}。请确保项目已正确编译，并且程序集已加载到应用程序域中。";
            throw new InvalidOperationException(errorMsg, ex);
        }

        builder.RegisterAssemblyTypes(assemblyApplication)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

        // 注册 Core.DataAccess 程序集中的所有类型（实现接口的类型，如 UnitOfWork）
        builder.RegisterAssemblyTypes(assemblyDataAccess)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

        // 注册 BaseRepo<T> 泛型类型
        var baseRepoType = FindGenericType(assemblyDataAccess, "BaseRepo`1");
        var iBaseRepoType = FindGenericType(assemblyDataAccess, "IBaseRepo`1");
        if (baseRepoType != null && iBaseRepoType != null)
        {
            builder.RegisterGeneric(baseRepoType)
                .As(iBaseRepoType)
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
        }
    }

    /// <summary>
    /// 在程序集中查找泛型类型
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <param name="typeName">类型名称（如 BaseRepo`1）</param>
    /// <returns>找到的类型，未找到返回null</returns>
    private static Type FindGenericType(Assembly assembly, string typeName)
    {
        try
        {
            var type = assembly.GetType(typeName);
            if (type != null) return type;

            foreach (var t in assembly.GetTypes())
            {
                if (t.IsGenericType && t.Name == typeName)
                {
                    return t;
                }
            }

            var fullNames = new[]
            {
                $"Core.Application.{typeName}",
                $"Core.DataAccess.{typeName}",
            };

            foreach (var fullName in fullNames)
            {
                type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaderExceptions = ex.LoaderExceptions
                .Where(e => e != null)
                .Select(e => e!.Message)
                .ToList();
            var errorMsg = $"加载程序集 {assembly.FullName} 中的类型时出错：{string.Join("; ", loaderExceptions)}";
            throw new InvalidOperationException(errorMsg, ex);
        }
        catch (Exception ex)
        {
            var errorMsg = $"查找类型 {typeName} 时出错：{ex.Message}";
            throw new InvalidOperationException(errorMsg, ex);
        }
    }
}
