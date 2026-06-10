using Microsoft.AspNetCore.Mvc.Filters;

namespace IdemX.Admin.Api.Filters
{
    /// <summary>
    /// 请求验证错误处理
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class RequiredError : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext actionContext)
        {
            // base.OnResultExecuting(actionContext);
            var modelState = actionContext.ModelState;
            List<ErrorView> errors = [];
            if (!modelState.IsValid)
            {
                var baseResult = new CustomApiResponse<List<ErrorView>>()
                {
                    Code = 1,
                    Msg = "请提交必要的参数",
                };
                foreach (var key in modelState.Keys)
                {
                    var state = modelState[key];
                    if (state.Errors.Any())
                    {
                        ErrorView errorView = new()
                        {
                            ErrorName = key,
                            Error = state.Errors.First().ErrorMessage
                        };
                        errors.Add(errorView);
                        baseResult.Msg += errorView.ErrorName + "-" + errorView.Error;
                    }
                }
                baseResult.Data = errors;
                // 重要：不要手动 JsonSerializer.Serialize（会绕过全局 JsonOptions，导致字段名大小写不一致）
                // 这里用 JsonResult 交给 MVC 序列化管线，确保输出与正常接口一致（code/msg/data/total）
                actionContext.Result = new JsonResult(baseResult)
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }
        }
    }
}
