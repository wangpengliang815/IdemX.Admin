namespace IdemX.Admin.Api.Controllers;

/// <summary>
/// 健康检查
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// 健康检查端点
    /// </summary>
    /// <returns>健康状态信息</returns>
    [HttpGet]
    public CustomApiResponse<object> Ping()
    {
        var healthInfo = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Service = "IdemX.Admin.Api",
            Version = "1.0.0"
        };

        return CustomApiResponse<object>.Ok("Pong...", healthInfo);
    }
}