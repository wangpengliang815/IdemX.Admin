namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 需登录的控制器基类；例外处自行加 AllowAnonymous。
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[RequiredError]
[Authorize]
public abstract class AuthorizedControllerBase : ControllerBase
{

}
