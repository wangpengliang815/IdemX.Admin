namespace Core.Model.System;

public class SysAreaGetByCodeReq
{
    [Required]
    public int Level { get; set; }

    [Required]
    public string ParentCode { get; set; }
}
