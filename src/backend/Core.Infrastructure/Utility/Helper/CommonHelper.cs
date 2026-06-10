using System.Security.Cryptography;

namespace Core.Infrastructure.Utility;

/// <summary>
/// 通用帮助类
/// </summary>
public static class CommonHelper
{
    /// <summary>
    /// 校验中国大陆 11 位手机号格式
    /// </summary>
    public static bool IsMobile(string mobilePhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(mobilePhoneNumber))
            return false;

        var phone = mobilePhoneNumber.Trim();
        if (phone.Length != 11 || phone[0] != '1' || phone[1] < '3' || phone[1] > '9')
            return false;

        for (var i = 0; i < phone.Length; i++)
        {
            if (!char.IsDigit(phone[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 检测是否符合email格式
    /// </summary>
    /// <param name="strEmail">要判断的email字符串</param>
    /// <returns>判断结果</returns>
    public static bool IsValidEmail(string strEmail)
    {
        return Regex.IsMatch(strEmail, @"^[\w\.]+([-]\w+)*@[A-Za-z0-9-_]+[\.][A-Za-z0-9-_]");
    }

    /// <summary>
    /// 检测是否是正确的Url
    /// </summary>
    /// <param name="strUrl">要验证的Url</param>
    /// <returns>判断结果</returns>
    public static bool IsUrl(string strUrl)
    {
        return Regex.IsMatch(strUrl,
            @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
    }

    public static int[] StringToIntArray(string str)
    {
        try
        {
            if (string.IsNullOrEmpty(str)) return new int[0];
            if (str.EndsWith(","))
            {
                str = str.Remove(str.Length - 1, 1);
            }
            var idstrarr = str.Split(',');
            var idintarr = new int[idstrarr.Length];

            for (int i = 0; i < idstrarr.Length; i++)
            {
                idintarr[i] = Convert.ToInt32(idstrarr[i]);
            }
            return idintarr;
        }
        catch
        {
            return new int[0];
        }
    }

    public static string[] StringToStringArray(string str)
    {
        try
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<string>();
            if (str.EndsWith(",")) str = str.Remove(str.Length - 1, 1);
            return str.Split(',');
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static long[] StringToLongArray(string str)
    {
        try
        {
            if (string.IsNullOrEmpty(str)) return [];
            if (str.EndsWith(','))
            {
                str = str.Remove(str.Length - 1, 1);
            }
            var idstrarr = str.Split(',');
            var idlongarr = new long[idstrarr.Length];

            for (int i = 0; i < idstrarr.Length; i++)
            {
                idlongarr[i] = long.Parse(idstrarr[i]);
            }
            return idlongarr;
        }
        catch
        {
            return [];
        }
    }

    public static long[] StringToLongArray(string[] str)
    {
        try
        {
            long[] iNums = Array.ConvertAll<string, long>(str, s => long.Parse(s));
            return iNums;
        }
        catch
        {
            return Array.Empty<long>();
        }
    }

    public static int[] StringArrAyToIntArray(string[] str)
    {
        try
        {
            int[] iNums = Array.ConvertAll<string, int>(str, s => int.Parse(s));
            return iNums;
        }
        catch
        {
            return Array.Empty<int>();
        }
    }

    public static Guid[] StringToGuidArray(string str)
    {
        try
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<Guid>();
            if (str.EndsWith(",")) str = str.Remove(str.Length - 1, 1);
            var strarr = str.Split(',');
            System.Guid[] guids = new System.Guid[strarr.Length];
            for (int index = 0; index < strarr.Length; index++)
            {
                guids[index] = System.Guid.Parse(strarr[index]);
            }
            return guids;
        }
        catch
        {
            return Array.Empty<Guid>();
        }
    }

    /// <summary>
    /// 通过创建哈希字符串适用于任何 MD5 哈希函数 （在任何平台） 上创建 32 个字符的十六进制格式哈希字符串
    /// </summary>
    /// <param name="source"></param>
    /// <returns>32位md5加密字符串</returns>
    public static string Md5For32(string source)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            string hash = sBuilder.ToString();
            return hash.ToUpper();
        }
    }

    /// <summary>
    /// 获取16位md5加密
    /// </summary>
    /// <param name="source"></param>
    /// <returns>16位md5加密字符串</returns>
    public static string Md5For16(string source)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
            //转换成字符串，并取9到25位
            string sBuilder = BitConverter.ToString(data, 4, 8);
            //BitConverter转换出来的字符串会在每个字符中间产生一个分隔符，需要去除掉
            sBuilder = sBuilder.Replace("-", "");
            return sBuilder.ToUpper();
        }
    }

    /// <summary>
    /// 返回当前的毫秒时间戳
    /// </summary>
    public static string Msectime()
    {
        long timeTicks = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        return timeTicks.ToString();
    }

    /// <summary>
    /// 剩余多久时间
    /// </summary>
    /// <param name="remainingTime"></param>
    /// <returns>文字描述</returns>
    public static string GetRemainingTime(DateTime remainingTime)
    {
        TimeSpan timeSpan = remainingTime - DateTime.Now;
        var day = timeSpan.Days;
        var hours = timeSpan.Hours;
        var minute = timeSpan.Minutes;
        var seconds = timeSpan.Seconds;
        if (day > 0)
        {
            return day + "天" + hours + "小时" + minute + "分" + seconds + "秒";
        }
        else
        {
            if (hours > 0)
            {
                return hours + "小时" + minute + "分" + seconds + "秒";
            }
            else
            {
                return minute + "分" + seconds + "秒";
            }
        }
    }

    /// <summary>
    /// 剩余多久时间
    /// </summary>
    /// <param name="remainingTime"></param>
    /// <returns>返回时间类型</returns>
    public static void GetBackTime(DateTime remainingTime, out int day, out int hours, out int minute, out int seconds)
    {
        TimeSpan timeSpan = remainingTime - DateTime.Now;
        day = timeSpan.Days;
        hours = timeSpan.Hours;
        minute = timeSpan.Minutes;
        seconds = timeSpan.Seconds;
    }

    /// <summary>
    /// 计算时间戳剩余多久时间
    /// </summary>
    /// <param name="postTime">提交时间(要是以前的时间)</param>
    /// <returns></returns>
    public static string TimeAgo(DateTime postTime)
    {
        //当前时间的时间戳
        var nowtimes = ConvertTicks(DateTime.Now);
        //提交的时间戳
        var posttimes = ConvertTicks(postTime);
        //相差时间戳
        var counttime = nowtimes - posttimes;

        //进行时间转换
        if (counttime <= 60)
        {
            return "刚刚";
        }
        else if (counttime > 60 && counttime <= 120)
        {
            return "1分钟前";
        }
        else if (counttime > 120 && counttime <= 180)
        {
            return "2分钟前";
        }
        else if (counttime > 180 && counttime < 3600)
        {
            return Convert.ToInt32((counttime / 60)) + "分钟前";
        }
        else if (counttime >= 3600 && counttime < 3600 * 24)
        {
            return Convert.ToInt32((counttime / 3600)) + "小时前";
        }
        else if (counttime >= 3600 * 24 && counttime < 3600 * 24 * 2)
        {
            return "昨天";
        }
        else if (counttime >= 3600 * 24 * 2 && counttime < 3600 * 24 * 3)
        {
            return "前天";
        }
        else if (counttime >= 3600 * 24 * 3 && counttime <= 3600 * 24 * 7)
        {
            return Convert.ToInt32((counttime / (3600 * 24))) + "天前";
        }
        else if (counttime >= 3600 * 24 * 7 && counttime <= 3600 * 24 * 30)
        {
            return Convert.ToInt32((counttime / (3600 * 24 * 7))) + "周前";
        }
        else if (counttime >= 3600 * 24 * 30 && counttime <= 3600 * 24 * 365)
        {
            return Convert.ToInt32((counttime / (3600 * 24 * 30))) + "个月前";
        }
        else if (counttime >= 3600 * 24 * 365)
        {
            return Convert.ToInt32((counttime / (3600 * 24 * 365))) + "年前";
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// 时间转换为秒的时间戳
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private static long ConvertTicks(DateTime time)
    {
        long currentTicks = time.Ticks;
        DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long currentMillis = (currentTicks - dtFrom.Ticks) / 10000000;  //转换为秒为Ticks/10000000，转换为毫秒Ticks/10000
        return currentMillis;
    }

    /// <summary>
    /// 清除HTML中指定样式
    /// </summary>
    /// <param name="content"></param>
    /// <param name="rule"></param>
    /// <returns></returns>
    public static string ClearHtml(string content, string[] rule)
    {
        if (!rule.Any())
        {
            return content;
        }

        foreach (var item in rule)
        {
            content = Regex.Replace(content, "/" + item + @"\s*=\s*\d+\s*/i", "");
            content = Regex.Replace(content, "/" + item + @"\s*=\s*.+?[""]/i", "");
            content = Regex.Replace(content, "/" + item + @"\s*:\s*\d+\s*px\s*;?/i", "");
        }
        return content;
    }

    /// <summary>
    /// list随机排序方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ListT"></param>
    /// <returns></returns>
    public static List<T> RandomSortList<T>(List<T> ListT)
    {
        Random random = new Random();
        List<T> newList = new List<T>();
        foreach (T item in ListT)
        {
            newList.Insert(random.Next(newList.Count + 1), item);
        }
        return newList;
    }

    /// <summary>
    /// 截前后字符(串)
    /// </summary>
    /// <param name="val">原字符串</param>
    /// <param name="str">要截掉的字符串</param>
    /// <param name="all">是否贪婪</param>
    /// <returns></returns>
    public static string GetCaptureInterceptedText(string val, string str, bool all = false)
    {
        return Regex.Replace(val, @"(^(" + str + ")" + (all ? "*" : "") + "|(" + str + ")" + (all ? "*" : "") + "$)", "");
    }

    /// <summary>
    /// 密码加密方法
    /// </summary>
    /// <param name="password">要加密的字符串</param>
    /// <param name="createTime">时间组合</param>
    /// <returns></returns>
    public static string EnPassword(string password, DateTime createTime)
    {
        var dtStr = createTime.ToString("yyyyMMddHHmmss");
        var md5 = Md5For32(password);
        var enPwd = Md5For32(md5 + dtStr);
        return enPwd;
    }

    /// <summary>
    /// 获取现在是星期几
    /// </summary>
    /// <returns></returns>
    public static string GetWeek()
    {
        string week = string.Empty;
        switch (DateTime.Now.DayOfWeek)
        {
            case DayOfWeek.Monday:
                week = "周一";
                break;
            case DayOfWeek.Tuesday:
                week = "周二";
                break;
            case DayOfWeek.Wednesday:
                week = "周三";
                break;
            case DayOfWeek.Thursday:
                week = "周四";
                break;
            case DayOfWeek.Friday:
                week = "周五";
                break;
            case DayOfWeek.Saturday:
                week = "周六";
                break;
            case DayOfWeek.Sunday:
                week = "周日";
                break;
            default:
                week = "N/A";
                break;
        }
        return week;
    }

    /// <summary>
    /// UrlEncode (URL编码)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string UrlEncode(string str)
    {
        StringBuilder sb = new();
        byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
        for (int i = 0; i < byStr.Length; i++)
        {
            sb.Append(@"%" + Convert.ToString(byStr[i], 16));
        }

        return (sb.ToString());
    }

    /// <summary>
    /// 获取10位时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStampByTotalSeconds()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    /// <summary>
    /// 获取13位时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStampByTotalMilliseconds()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }

    /// <summary>
    /// 去除字符串中的空格
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string RemoveWhitespace(this string input)
    {
        return string.IsNullOrEmpty(input) ? null : Regex.Replace(input, @"\s+", "");
    }

    private static readonly Dictionary<string, (string Extension, string ContentType)> OssTempImageTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = (".jpg", "image/jpeg"),
            ["image/jpg"] = (".jpg", "image/jpeg"),
            ["image/png"] = (".png", "image/png"),
            ["image/gif"] = (".gif", "image/gif"),
            ["image/webp"] = (".webp", "image/webp"),
        };

    /// <summary>
    /// OSS 暂存上传允许的 multipart 图片：由 Content-Type 解析扩展名与规范 MIME
    /// </summary>
    public static bool TryGetOssTempImageUploadMeta(string contentType, out string extension, out string normalizedContentType)
    {
        extension = null;
        normalizedContentType = null;
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        if (!OssTempImageTypes.TryGetValue(contentType.Trim(), out var meta))
            return false;

        extension = meta.Extension;
        normalizedContentType = meta.ContentType;
        return true;
    }

    /// <summary>
    /// 从Base64 Data URL中解析图片数据并获取文件扩展名
    /// </summary>
    /// <param name="base64Url">Base64 Data URL (格式: data:image/[type];base64,[data])</param>
    /// <returns>包含Base64数据、MIME类型和文件扩展名的元组</returns>
    public static (string Base64Data, string MimeType, string FileExtension) ParseBase64ImageUrl(string base64Url)
    {
        if (string.IsNullOrEmpty(base64Url))
            return (null, null, null);

        // 匹配 data:image/[type];base64,[data] 格式
        var match = Regex.Match(base64Url, @"^data:(?<mimeType>image\/[a-zA-Z\+\-]+);base64,(?<data>.+)$", RegexOptions.IgnoreCase);

        if (!match.Success)
            return (null, null, null);

        var mimeType = match.Groups["mimeType"].Value.ToLower();
        var base64Data = match.Groups["data"].Value;

        // 根据MIME类型确定文件扩展名
        var extension = mimeType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            _ => ".jpg"
        };

        return (base64Data, mimeType, extension);
    }
}
