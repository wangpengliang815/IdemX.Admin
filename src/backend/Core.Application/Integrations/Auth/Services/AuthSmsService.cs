namespace Core.Application;

/// <summary>
/// 认证相关短信应用服务（找回密码）
/// </summary>
public class AuthSmsService(
    IBaseRepo<SysUser> userRepo,
    ISmsProvider smsProvider,
    ISmsCodeVerification smsCodeVerification) : IAuthSmsService
{
    private const string SmsPurposeResetPassword = "resetpwd";

    /// <summary>
    /// 向已绑定手机号发送找回密码验证码
    /// </summary>
    public async Task<CustomApiResponse> SendForgotPasswordSmsAsync(SmsSendCodeReq request)
    {
        var user = await userRepo.GetFirstAsync(p => p.Phone == request.PhoneNumber);
        if (user is null)
            return CustomApiResponse.Fail(GlobalConstVars.PhoneNotRegistered);

        return await SendVerificationCodeAsync(request.PhoneNumber, SmsPurposeResetPassword);
    }

    /// <summary>
    /// 校验找回密码短信码并更新密码
    /// </summary>
    public async Task<CustomApiResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneReq request)
    {
        var user = await userRepo.GetFirstAsync(p => p.Phone == request.PhoneNumber);
        if (user is null)
            return CustomApiResponse.Fail(GlobalConstVars.PhoneNotRegistered);

        if (!smsCodeVerification.Verify(request.PhoneNumber, request.SmsCode, SmsPurposeResetPassword))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneSmsInvalid);

        if (PasswordHelper.TryVerify(request.NewPassword, user.Password, out _))
            return CustomApiResponse.Fail(GlobalConstVars.SameAsOldPassword);

        user.Password = PasswordHelper.Hash(request.NewPassword);

        var ok = await userRepo.EditAsync(user);
        return ok
            ? CustomApiResponse.Ok(GlobalConstVars.ResetPasswordSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.ResetPasswordFailure);
    }

    private async Task<CustomApiResponse> SendVerificationCodeAsync(string phone, string purpose)
    {
        if (!smsCodeVerification.CanSend(phone))
            return CustomApiResponse.Fail("验证码发送过于频繁，请稍后再试");

        var code = Random.Shared.Next(100000, 999999).ToString();
        var sendResult = await smsProvider.SendVerificationCodeAsync(phone, code);
        if (sendResult.Status != SmsSendStatus.Success)
            return CustomApiResponse.Fail(sendResult.Message);

        smsCodeVerification.SaveSentCode(phone, code, purpose);
        return CustomApiResponse.Ok(sendResult.Message);
    }
}
