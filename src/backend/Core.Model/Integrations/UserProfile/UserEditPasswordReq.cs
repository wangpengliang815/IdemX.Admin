namespace Core.Model.UserProfile;

public class UserEditPasswordReq
{
    public string OldPassword { get; set; }

    public string Password { get; set; }

    public string Repassword { get; set; }
}