namespace Core.Model.Auth;

public class LoginByPasswordReq
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}