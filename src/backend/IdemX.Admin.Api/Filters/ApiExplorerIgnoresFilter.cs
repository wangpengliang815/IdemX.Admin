namespace IdemX.Admin.Api.Filters
{
    /// <summary>
    /// 自带的Controller与swagger3.0冲突，在此排除扫描
    /// </summary>
    public class ApiExplorerIgnoresFilter : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            string[] excludeApiName = [];

            if (excludeApiName.Contains(action.Controller.ControllerName))
                action.ApiExplorer.IsVisible = false;
        }
    }
}
