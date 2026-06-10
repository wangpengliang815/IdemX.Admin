namespace IdemX.Admin.Api.Auth
{
    public class ApiResponseHandler(IOptionsMonitor<AuthenticationSchemeOptions> options
            , ILoggerFactory logger
            , UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "application/json";

            var response = new
            {
                code = 1401,
                msg = "很抱歉，您无权访问该接口，请确保已经登录!"
            };
            await Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "application/json";

            var response = new
            {
                code = 1403,
                msg = "很抱歉，您的访问权限等级不够，请联系管理员!"
            };
            await Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

