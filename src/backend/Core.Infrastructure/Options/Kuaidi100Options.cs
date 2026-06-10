namespace Core.Infrastructure.Options;

/// <summary>
/// 快递100 配置（appsettings 节名：Kuaidi100Options）
/// </summary>
public class Kuaidi100Options
{
    /// <summary>
    /// 是否启用快递100接口
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 授权码 customer（企业版）
    /// </summary>
    public string Customer { get; set; }

    /// <summary>
    /// 密钥 key（签名：MD5(param + key + customer) 转大写）
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 实时查询接口地址
    /// </summary>
    public string QueryUrl { get; set; } = "https://poll.kuaidi100.com/poll/query.do";

    /// <summary>
    /// param.resultv2：1=轨迹含物流状态名称（见快递100文档）
    /// </summary>
    public string ResultV2 { get; set; } = "1";

    /// <summary>
    /// 同一运单轨迹查询结果缓存分钟数（快递100 要求每单查询间隔≥30 分钟，默认 40）
    /// </summary>
    public int TrackCacheMinutes { get; set; } = 40;
}
