namespace Core.Model.Shared;

/// <summary>
/// DTO 基类，与 BaseEntity 审计字段对齐，便于 AutoMapper 映射及前端展示创建/更新时间。
/// </summary>
public abstract class BaseResp
{
    /// <summary>
    /// 主键 Id（雪花 ID，序列化为 string）
    /// </summary>
    public virtual long Id { get; set; }

    /// <summary>
    /// 更新时间（序列化格式 yyyy-MM-dd HH:mm:ss，未更新时为 null）
    /// </summary>
    public virtual DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建人Id
    /// </summary>
    public virtual long? CreateBy { get; set; }

    /// <summary>
    /// 创建人用户名（与 BaseEntity.CreateByUsername 对齐）
    /// </summary>
    public virtual string CreateByUsername { get; set; }

    /// <summary>
    /// 创建时间（序列化格式 yyyy-MM-dd HH:mm:ss）
    /// </summary>
    public virtual DateTime CreateTime { get; set; }
}

