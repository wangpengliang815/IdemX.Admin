namespace Core.Model.Shared;

public class BaseAuditReq
{
    [Required]
    public long Id { get; set; }

    /// <summary>
    /// 审核说明
    /// </summary>
    public string AuditRemark { get; set; }

    /// <summary>
    /// 审核操作 true 通过 false 拒绝 
    /// </summary>
    [Required]
    public bool IsApproved { get; set; }
}
