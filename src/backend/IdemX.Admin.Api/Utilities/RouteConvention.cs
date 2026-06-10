namespace IdemX.Admin.Api.Utilities;

/// <summary>
/// 全局约定：将 ControllerName / ActionName 改为小写开头（用于 [controller]/[action] token 输出）
/// </summary>
public sealed class RouteConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            controller.ControllerName = LowercaseFirstChar(controller.ControllerName);

            foreach (var action in controller.Actions)
            {
                action.ActionName = LowercaseFirstChar(action.ActionName);
            }
        }
    }

    private static string LowercaseFirstChar(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var first = name[0];
        if (!char.IsLetter(first) || char.IsLower(first))
        {
            return name;
        }

        return char.ToLowerInvariant(first) + name[1..];
    }
}


