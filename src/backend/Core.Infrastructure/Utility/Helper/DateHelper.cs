namespace Core.Infrastructure.Utility;

public class DateHelper
{
    /// <summary>
    /// 时间戳转时间
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTime StampToDateTime(string time)
    {
        time = time.Substring(0, 10);
        double timestamp = Convert.ToInt64(time);
        System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();
        return dateTime;
    }

    /// <summary>
    /// Unix 毫秒时间戳转本地时间；非法或越界返回 false。与 StampToDateTime（秒级、字符串截取）语义不同，供 e 签宝等毫秒 long 字段使用。
    /// </summary>
    public static bool TryUnixMillisecondsToLocal(
        long unixMs,
        out DateTime localTime,
        out string errorMessage,
        string invalidRangeMessage = "")
    {
        localTime = default;
        errorMessage = string.IsNullOrEmpty(invalidRangeMessage)
            ? "timestamp 非法，无法转换为事件时间"
            : invalidRangeMessage;
        if (unixMs <= 0 || unixMs > DateTimeOffset.MaxValue.ToUnixTimeMilliseconds())
            return false;

        localTime = DateTimeOffset.FromUnixTimeMilliseconds(unixMs).LocalDateTime;
        return true;
    }

    /// <summary>
    /// 时间戳转时间：毫秒级
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime GetDateTimeMilliseconds(long timestamp)
    {
        long begtime = timestamp * 10000;
        DateTime dt_1970 = new DateTime(1970, 1, 1, 8, 0, 0);
        long tricks_1970 = dt_1970.Ticks;//1970年1月1日刻度
        long time_tricks = tricks_1970 + begtime;//日志日期刻度
        DateTime dt = new DateTime(time_tricks);//转化为DateTime
        return dt;
    }

    /// <summary>
    /// 计算时间相差几天
    /// </summary>
    /// <param name="dateStart"></param>
    /// <param name="dateEnd"></param>
    /// <returns></returns>
    public static int DateDiff(DateTime dateStart, DateTime dateEnd)
    {
        DateTime start = Convert.ToDateTime(dateStart.ToShortDateString());

        DateTime end = Convert.ToDateTime(dateEnd.ToShortDateString());

        TimeSpan sp = end.Subtract(start);

        return sp.Days;
    }
}
