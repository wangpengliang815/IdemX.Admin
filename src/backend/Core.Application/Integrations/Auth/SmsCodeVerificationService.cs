namespace Core.Application;

/// <summary>
/// 短信验证码频控、缓存与校验（调用方使用；发信由 ISmsProvider 负责）
/// </summary>
public class SmsCodeVerificationService(ITairProvider tairProvider) : ISmsCodeVerification
{
    private const int CodeExpirationMinutes = 10;

    private const int SendIntervalSeconds = 60;

    private const int DailyLimit = 10;

    private const int MaxErrorAttempts = 5;

    public bool CanSend(string phoneNumber)
    {
        if (TryGetLastSendTime(LastSendKey(phoneNumber), out DateTime lastSendTime))
        {
            var seconds = (DateTime.Now - lastSendTime).TotalSeconds;
            if (seconds < SendIntervalSeconds)
                return false;
        }

        return GetCounter(TodayCountKey(phoneNumber)) < DailyLimit;
    }

    public void SaveSentCode(string phoneNumber, string code, string purpose = null)
    {
        var codeKey = CodeKey(phoneNumber, purpose);
        tairProvider.SetString(codeKey, code, TimeSpan.FromMinutes(CodeExpirationMinutes));

        tairProvider.SetString(LastSendKey(phoneNumber), DateTime.Now.ToString("O"), TimeSpan.FromMinutes(1));

        var todayKey = TodayCountKey(phoneNumber);
        var todayCount = GetCounter(todayKey) + 1;
        tairProvider.SetString(todayKey, todayCount.ToString(), TimeSpan.FromDays(1));
    }

    public bool Verify(string phoneNumber, string code, string purpose = null, bool consumeOnSuccess = true)
    {
        var codeKey = CodeKey(phoneNumber, purpose);
        if (!tairProvider.TryGetString(codeKey, out string cachedCode))
            return false;

        if (cachedCode != code)
        {
            RecordError(phoneNumber, purpose);
            return false;
        }

        if (consumeOnSuccess)
            tairProvider.Remove(codeKey);
        ClearErrorRecord(phoneNumber, purpose);

        return true;
    }

    private static string CodeKey(string phoneNumber, string purpose) =>
        string.IsNullOrEmpty(purpose) ? $"SmsCode_{phoneNumber}" : $"SmsCode_{purpose}_{phoneNumber}";

    private static string ErrorKey(string phoneNumber, string purpose) =>
        string.IsNullOrEmpty(purpose) ? $"SmsError_{phoneNumber}" : $"SmsError_{purpose}_{phoneNumber}";

    private static string LastSendKey(string phoneNumber) => $"SmsLastSend_{phoneNumber}";

    private static string TodayCountKey(string phoneNumber) =>
        $"SmsCount_{phoneNumber}_{DateTime.Now:yyyyMMdd}";

    private void RecordError(string phoneNumber, string purpose)
    {
        var errorKey = ErrorKey(phoneNumber, purpose);
        var errorCount = GetCounter(errorKey) + 1;
        tairProvider.SetString(errorKey, errorCount.ToString(), TimeSpan.FromMinutes(10));

        if (errorCount >= MaxErrorAttempts)
            tairProvider.Remove(CodeKey(phoneNumber, purpose));
    }

    private void ClearErrorRecord(string phoneNumber, string purpose) =>
        tairProvider.Remove(ErrorKey(phoneNumber, purpose));

    private bool TryGetLastSendTime(string key, out DateTime lastSendTime)
    {
        lastSendTime = default;
        if (!tairProvider.TryGetString(key, out var text))
            return false;

        return DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.RoundtripKind, out lastSendTime);
    }

    private long GetCounter(string key)
    {
        if (!tairProvider.TryGetString(key, out var text))
            return 0;

        return long.TryParse(text, out var count) ? count : 0;
    }
}
