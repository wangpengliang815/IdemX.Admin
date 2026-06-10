namespace Core.Application;

/// <summary>
/// 认证相关短信应用服务
/// </summary>
public class AuthSmsService(
    IBaseRepo<SysUser> userRepo,
    ISmsProvider smsProvider,
    ISmsCodeVerification smsCodeVerification) : IAuthSmsService
{
    private const string SmsPurposeResetPassword = "resetpwd";
    private const string SmsSceneLogin = "login";
    private const string SmsSceneRegister = "register";

    /// <summary>
    /// 查询手机号是否已有用户
    /// </summary>
    public async Task<CustomApiResponse<bool>> CheckPhoneExistsAsync(SmsSendCodeReq request)
    {
        var phone = request.PhoneNumber?.Trim();
        if (!CommonHelper.IsMobile(phone))
            return CustomApiResponse<bool>.Fail("请输入正确的手机号", false);

        var existPhone = await userRepo.IsAnyAsync(p => p.Phone == phone);
        return CustomApiResponse<bool>.Ok(existPhone ? GlobalConstVars.DataIsHave : GlobalConstVars.DataIsNo, existPhone);
    }

    /// <summary>
    /// 按场景发送登录或注册验证码
    /// </summary>
    public async Task<CustomApiResponse> SendSmsCodeAsync(SmsSendCodeReq request)
    {
        var phone = request.PhoneNumber?.Trim();
        if (!CommonHelper.IsMobile(phone))
            return CustomApiResponse.Fail("请输入正确的手机号");

        var scene = request.Scene?.Trim().ToLowerInvariant();
        if (scene is not (SmsSceneLogin or SmsSceneRegister))
            return CustomApiResponse.Fail("无效的发码场景");

        var phoneRegistered = await userRepo.IsAnyAsync(p => p.Phone == phone);
        if (scene == SmsSceneLogin && !phoneRegistered)
            return CustomApiResponse.Fail(GlobalConstVars.PhoneNotRegistered);

        if (scene == SmsSceneRegister && phoneRegistered)
            return CustomApiResponse.Fail(GlobalConstVars.RegPhoneExists);

        return await SendVerificationCodeAsync(phone, purpose: null);
    }

    /// <summary>
    /// 向已注册用户发送找回密码验证码
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
