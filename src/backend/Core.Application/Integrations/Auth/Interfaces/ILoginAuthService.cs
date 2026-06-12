namespace Core.Application;

public interface ILoginAuthService
{
    Task<CustomApiResponse<string>> LoginByPasswordAsync(LoginByPasswordReq request);

    Task<CustomApiResponse> LogoutAsync();
}
