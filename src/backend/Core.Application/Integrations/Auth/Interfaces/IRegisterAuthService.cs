namespace Core.Application;

public interface IRegisterAuthService
{
    Task<CustomApiResponse> RegisterAsync(SysUserRegReq request);
}
