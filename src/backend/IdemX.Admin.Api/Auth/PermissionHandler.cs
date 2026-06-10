namespace IdemX.Admin.Api.Auth
{
    /// <summary>
    /// 权限验证处理器：完成 HTTP 认证后委托 Application 做路由权限与 Token 校验
    /// </summary>
    public class PermissionHandler(
        IAuthenticationSchemeProvider schemes,
        IPermissionEvaluator permissionEvaluator,
        IHttpContextAccessor accessor) : AuthorizationHandler<PermissionRequirement>
    {
        public IAuthenticationSchemeProvider Schemes { get; } = schemes;

        /// <summary>
        /// 授权核心流程：认证用户后校验角色路由权限与 Token
        /// </summary>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var httpContext = accessor.HttpContext;

            if (httpContext is null)
            {
                context.Succeed(requirement);
                return;
            }

            await permissionEvaluator.EnsurePermissionsLoadedAsync();

            var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
            {
                if (await handlers.GetHandlerAsync(httpContext, scheme.Name) is IAuthenticationRequestHandler handler
                    && await handler.HandleRequestAsync())
                {
                    context.Fail();
                    return;
                }
            }

            var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate is null)
            {
                context.Fail();
                return;
            }

            var authResult = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
            if (authResult?.Principal is null)
            {
                context.Fail();
                return;
            }

            httpContext.User = authResult.Principal;

            var requestPath = httpContext.Request.Path.Value?.ToLower() ?? string.Empty;

            if (!permissionEvaluator.HasRouteAccess(requestPath, httpContext.User, requirement.ClaimType))
            {
                context.Fail();
                return;
            }

            if (!permissionEvaluator.IsTokenValid(httpContext.User))
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}
