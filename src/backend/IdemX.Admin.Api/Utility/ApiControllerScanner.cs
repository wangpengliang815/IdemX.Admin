using System.ComponentModel;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;

namespace IdemX.Admin.Api.Utility;

/// <summary>
/// 反射扫描本 Api 项目 Controller/Action，供菜单权限配置使用
/// </summary>
public static class ApiControllerScanner
{
    private const string ControllerSuffix = "Controller";
    private const int ControllerSuffixLength = 10;
    private const int MinControllerNameLength = 11;
    private const string ActionType = "action";
    private const string ControllerType = "controller";
    private const string ActionResultGenericTypeName = "ActionResult`1";

    private static readonly HashSet<string> ExcludedControllers =
    [
        "AuthController",
        "ToolsController",
        "UserProfileController",
        "InitController",
        "HealthController",
    ];

    private static readonly HashSet<string> ExcludedActions =
    [
        "ValidationProblem",
        "Json",
        "Dispose",
    ];

    private static readonly HashSet<string> ValidReturnTypes =
    [
        "ActionResult",
        "FileResult",
        "JsonResult",
        "CustomApiResponse",
        "IActionResult",
    ];

    public static List<ControllerPermission> GetAllControllerAndAction()
    {
        var result = new List<ControllerPermission>();
        var assembly = typeof(Program).Assembly;

        foreach (var type in GetControllerTypes(assembly))
        {
            var controller = CreateControllerPermission(type);
            if (controller is not null)
                result.Add(controller);
        }

        return result;
    }

    static IEnumerable<Type> GetControllerTypes(Assembly assembly) =>
        assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsClass: true } &&
                           type.Name.EndsWith(ControllerSuffix) &&
                           type.Name.Length > MinControllerNameLength &&
                           typeof(ControllerBase).IsAssignableFrom(type) &&
                           type != typeof(ControllerBase) &&
                           !ExcludedControllers.Contains(type.Name));

    static ControllerPermission CreateControllerPermission(Type controllerType)
    {
        var controllerName = controllerType.Name[..^ControllerSuffixLength];
        var description = controllerType.GetCustomAttribute<DescriptionAttribute>()?.Description;

        return new ControllerPermission
        {
            Name = controllerName,
            Description = description,
            Type = ControllerType,
            Action = GetActionPermissions(controllerType, controllerName),
        };
    }

    static List<ActionPermission> GetActionPermissions(Type controllerType, string controllerName)
    {
        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var actionPermissions = new List<ActionPermission>();

        foreach (var method in methods)
        {
            if (IsValidAction(method))
                actionPermissions.Add(CreateActionPermission(method, controllerName));
        }

        return actionPermissions.Distinct(new ActionPermissionComparer()).ToList();
    }

    static bool IsValidAction(MethodInfo method)
    {
        if (ExcludedActions.Contains(method.Name) ||
            !method.IsPublic ||
            method.IsGenericMethod ||
            method.IsSpecialName ||
            method.DeclaringType == typeof(object))
            return false;

        return IsValidReturnType(method.ReturnType);
    }

    static bool IsValidReturnType(Type returnType)
    {
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            returnType = returnType.GenericTypeArguments[0];

        if (ValidReturnTypes.Contains(returnType.Name) ||
            ValidReturnTypes.Contains(GetBaseTypeName(returnType)))
            return true;

        if (returnType.IsGenericType && returnType.GenericTypeArguments.Length > 0)
        {
            var genericArgument = returnType.GenericTypeArguments[0];

            if (ValidReturnTypes.Contains(genericArgument.Name) ||
                ValidReturnTypes.Contains(GetBaseTypeName(genericArgument)))
                return true;

            if (returnType.GetGenericTypeDefinition().Name == ActionResultGenericTypeName)
                return true;
        }

        return false;

        static string GetBaseTypeName(Type type)
        {
            var name = type.Name;
            var index = name.IndexOf('`');
            return index > 0 ? name[..index] : name;
        }
    }

    static ActionPermission CreateActionPermission(MethodInfo method, string controllerName)
    {
        var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
        var actionName = method.Name;

        return new ActionPermission
        {
            Name = actionName.ToCamelCase(),
            ActionName = actionName.ToCamelCase(),
            ControllerName = controllerName.ToCamelCase(),
            Description = description,
            Type = ActionType,
        };
    }

    sealed class ActionPermissionComparer : IEqualityComparer<ActionPermission>
    {
        public bool Equals(ActionPermission x, ActionPermission y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(ActionPermission obj) =>
            obj.Name?.ToUpperInvariant().GetHashCode() ?? 0;
    }
}

public class ActionPermission
{
    public string Name { get; set; }

    public string ControllerName { get; set; }

    public string ActionName { get; set; }

    public string Description { get; set; }

    public string Type { get; set; }
}

public class ControllerPermission
{
    public string Name { get; set; }

    public string Description { get; set; }

    public IList<ActionPermission> Action { get; set; }

    public string Type { get; set; }
}
