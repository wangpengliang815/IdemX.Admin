namespace Core.Application;

/// <summary>
/// 当前用户账号安全：改密与换绑手机
/// </summary>
public class UserProfileAccountService(
    IBaseRepo<SysUser> userRepo,
    ISmsProvider smsProvider,
    ISmsCodeVerification smsCodeVerification,
    IHttpContextUser contextUser) : IUserProfileAccountService
{
    private const string SmsPurposeChangePhone = "changephone";

    private const string SmsPurposeChangePhoneNew = "changephone_new";

    /// <summary>
    /// 修改当前用户登录密码（兼容旧 MD5；新密码须与旧密码不同）
    /// </summary>
    public async Task<CustomApiResponse<bool>> EditUserPasswordAsync(UserEditPasswordReq request)
    {
        var userModel = await userRepo.GetByIdAsync(contextUser.Id);
        if (userModel is null)
            return CustomApiResponse<bool>.Fail(GlobalConstVars.DataIsNo);

        if (!PasswordHelper.TryVerify(request.OldPassword, userModel.Password, out _))
            return CustomApiResponse<bool>.Fail(GlobalConstVars.OldPasswordError);

        if (PasswordHelper.TryVerify(request.Password, userModel.Password, out _))
            return CustomApiResponse<bool>.Fail(GlobalConstVars.SameAsOldPassword);

        userModel.Password = PasswordHelper.Hash(request.Password);
        var result = await userRepo.EditAsync(userModel);

        return result
            ? CustomApiResponse<bool>.Ok(GlobalConstVars.PasswordEditSuccess)
            : CustomApiResponse<bool>.Fail(GlobalConstVars.PasswordEditFailure);
    }

    /// <summary>
    /// 向当前账号已绑定手机号发送换绑验证码（须已绑定手机）
    /// </summary>
    public async Task<CustomApiResponse> SendChangePhoneSmsAsync()
    {
        var user = await userRepo.GetByIdAsync(contextUser.Id);
        if (user is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (string.IsNullOrWhiteSpace(user.Phone))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneNotBound);

        return await SendVerificationCodeAsync(user.Phone, SmsPurposeChangePhone);
    }

    /// <summary>
    /// 向换绑目标手机号发送短信验证码（须与当前绑定号不同且未被他人占用）
    /// </summary>
    public async Task<CustomApiResponse> SendChangePhoneSmsToNewAsync(UserSendChangePhoneNewSmsReq request)
    {
        var user = await userRepo.GetByIdAsync(contextUser.Id);
        if (user is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (string.IsNullOrWhiteSpace(user.Phone))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneNotBound);

        if (string.Equals(user.Phone, request.Phone, StringComparison.Ordinal))
            return CustomApiResponse.Fail(GlobalConstVars.SameAsCurrentPhone);

        var existingUser = await userRepo.GetFirstAsync(p => p.Phone == request.Phone && p.Id != user.Id);
        if (existingUser is not null)
            return CustomApiResponse.Fail(GlobalConstVars.RegPhoneExists);

        return await SendVerificationCodeAsync(request.Phone, SmsPurposeChangePhoneNew);
    }

    /// <summary>
    /// 校验当前绑定手机号收到的换绑验证码（不消费验证码、不写库，仅校验通过与否）
    /// </summary>
    public async Task<CustomApiResponse> VerifyChangePhoneSmsAsync(UserVerifyChangePhoneReq request)
    {
        var user = await userRepo.GetByIdAsync(contextUser.Id);
        if (user is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (string.IsNullOrWhiteSpace(user.Phone))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneNotBound);

        var smsOk = smsCodeVerification.Verify(user.Phone, request.SmsCode, SmsPurposeChangePhone, false);
        return smsOk
            ? CustomApiResponse.Ok(GlobalConstVars.PhoneVerifySuccess)
            : CustomApiResponse.Fail(GlobalConstVars.PhoneSmsInvalid);
    }

    /// <summary>
    /// 更换绑定手机号：先核销新号验证码（预检不消费），再核销旧号验证码，最后再次核销新号验证码并写库
    /// </summary>
    public async Task<CustomApiResponse> EditUserPhoneAsync(UserEditPhoneReq request)
    {
        var user = await userRepo.GetByIdAsync(contextUser.Id);
        if (user is null)
            return CustomApiResponse.Fail(GlobalConstVars.DataIsNo);

        if (string.IsNullOrWhiteSpace(user.Phone))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneNotBound);

        if (string.IsNullOrWhiteSpace(request.NewPhoneSmsCode))
            return CustomApiResponse.Fail(GlobalConstVars.DataParameterError);

        if (string.Equals(user.Phone, request.Phone, StringComparison.Ordinal))
            return CustomApiResponse.Fail(GlobalConstVars.SameAsCurrentPhone);

        var existingUser = await userRepo.GetFirstAsync(p => p.Phone == request.Phone && p.Id != user.Id);
        if (existingUser is not null)
            return CustomApiResponse.Fail(GlobalConstVars.RegPhoneExists);

        if (!smsCodeVerification.Verify(request.Phone, request.NewPhoneSmsCode, SmsPurposeChangePhoneNew, false))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneSmsInvalid);

        if (!smsCodeVerification.Verify(user.Phone, request.SmsCode, SmsPurposeChangePhone, true))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneSmsInvalid);

        if (!smsCodeVerification.Verify(request.Phone, request.NewPhoneSmsCode, SmsPurposeChangePhoneNew, true))
            return CustomApiResponse.Fail(GlobalConstVars.PhoneSmsInvalid);

        user.Phone = request.Phone;
        user.UpdateTime = DateTime.Now;

        var ok = await userRepo.EditAsync(user);
        return ok
            ? CustomApiResponse.Ok(GlobalConstVars.EditSuccess)
            : CustomApiResponse.Fail(GlobalConstVars.EditFailure);
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
