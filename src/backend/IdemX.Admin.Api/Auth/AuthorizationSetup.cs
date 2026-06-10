namespace IdemX.Admin.Api.Auth
{
    public static class AuthorizationSetup
    {
        private const string DENIED_ACTION_PATH = "/api/denied";

        private static readonly TimeSpan TOKEN_EXPIRATION = TimeSpan.FromHours(4);

        private const int CLOCK_SKEW_SECONDS = 60;

        private const string API_RESPONSE_HANDLER = nameof(ApiResponseHandler);

        private const string BEARER_PREFIX = "Bearer ";

        private static class ResponseHeaders
        {
            public const string TOKEN_ERROR = "Token-Error";

            public const string TOKEN_ERROR_ISSUER = "Token-Error-Iss";

            public const string TOKEN_ERROR_AUDIENCE = "Token-Error-Aud";

            public const string TOKEN_EXPIRED = "Token-Expired";
        }

        private static class ErrorMessages
        {
            public const string ISSUER_ERROR = "issuer is wrong!";

            public const string AUDIENCE_ERROR = "Audience is wrong!";
        }

        public static void AddAuthorizationSetup(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var secretKey = configuration["JwtConfig:SecretKey"] ?? string.Empty;
            var keyByteArray = Encoding.ASCII.GetBytes(secretKey);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            var issuer = configuration["JwtConfig:Issuer"];
            var audience = configuration["JwtConfig:Audience"];
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwtSigningSettings = new JwtSigningSettings
            {
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = signingCredentials,
                Expiration = TOKEN_EXPIRATION,
            };

            var roleRoutePermissionStore = new RoleRoutePermissionStore();
            var permissionRequirement = new PermissionRequirement(ClaimTypes.Role);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(CLOCK_SKEW_SECONDS),
                RequireExpirationTime = true,
            };

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.Name, policy => policy.Requirements.Add(permissionRequirement));
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = API_RESPONSE_HANDLER;
                options.DefaultForbidScheme = API_RESPONSE_HANDLER;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        if (!string.IsNullOrEmpty(context.ErrorDescription))
                        {
                            context.Response.Headers.Append(ResponseHeaders.TOKEN_ERROR, context.ErrorDescription);
                        }
                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        try
                        {
                            var authHeader = context.Request.Headers.Authorization.ToString();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith(BEARER_PREFIX))
                            {
                                var token = authHeader.Substring(BEARER_PREFIX.Length);
                                if (!string.IsNullOrEmpty(token))
                                {
                                    var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

                                    if (jwtToken.Issuer != issuer)
                                    {
                                        context.Response.Headers[ResponseHeaders.TOKEN_ERROR_ISSUER] = ErrorMessages.ISSUER_ERROR;
                                    }

                                    if (jwtToken.Audiences.FirstOrDefault() != audience)
                                    {
                                        context.Response.Headers[ResponseHeaders.TOKEN_ERROR_AUDIENCE] = ErrorMessages.AUDIENCE_ERROR;
                                    }
                                }
                            }

                            if (context.Exception?.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Append(ResponseHeaders.TOKEN_EXPIRED, "true");
                            }
                        }
                        catch
                        {
                        }

                        return Task.CompletedTask;
                    }
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(API_RESPONSE_HANDLER, _ => { });

            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton(jwtSigningSettings);
            services.AddSingleton(roleRoutePermissionStore);
            services.AddSingleton(permissionRequirement);
            services.AddSingleton<IPermissionCacheInvalidator>(sp => sp.GetRequiredService<RoleRoutePermissionStore>());
        }
    }
}

