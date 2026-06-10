namespace Core.Model.Shared;

/// <summary>
/// 实现该接口，表示实体有逻辑删除字段，可通过sqlsuagr实现自动过滤
/// </summary>
public interface IDeleted
{
    bool IsDeleted { get; set; }
}

/// <summary>
/// 新基类：雪花ID + 完整审计字段
/// </summary>
public abstract class BaseEntity : IDeleted
{
    /// <summary>
    /// 主键Id（雪花ID）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    [Required]
    public long Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(IsOnlyIgnoreUpdate = true)]
    [Required]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 创建人id
    /// </summary>
    [SugarColumn(IsOnlyIgnoreUpdate = true, IsNullable = true)]
    public long? CreateBy { get; set; }

    /// <summary>
    /// 创建人用户名（不可变，随 CreateBy 同步写入）
    /// </summary>
    [SugarColumn(IsOnlyIgnoreUpdate = true, IsNullable = true, Length = 64)]
    public string CreateByUsername { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [SugarColumn(IsNullable = true, IsOnlyIgnoreInsert = true)]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 修改人ID
    /// </summary>
    [SugarColumn(IsNullable = true, IsOnlyIgnoreInsert = true)]
    public long? UpdateBy { get; set; }

    /// <summary>
    /// 修改人用户名
    /// </summary>
    [SugarColumn(IsNullable = true, IsOnlyIgnoreInsert = true, Length = 64)]
    public string UpdateByUsername { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 删除时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除人ID
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public long? DeleteBy { get; set; }

    /// <summary>
    /// 删除人用户名
    /// </summary>
    [SugarColumn(IsNullable = true, Length = 64)]
    public string DeleteByUsername { get; set; }
}

