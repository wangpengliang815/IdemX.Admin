namespace Core.Application;

public interface ILoginAuthService
{
    Task<CustomApiResponse<string>> LoginByPasswordAsync(LoginByPasswordReq request);

    Task<CustomApiResponse<string>> LoginByPhoneAsync(LoginByPhoneReq request);

    Task<CustomApiResponse> LogoutAsync();
}
