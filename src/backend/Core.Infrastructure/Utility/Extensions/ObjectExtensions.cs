namespace Core.Infrastructure.Utility;

/// <summary>
/// 数据转换扩展方法
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// 数据转换为int类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static int ObjectToInt(this object thisValue)
    {
        int result = 0;
        if (thisValue == null)
            return 0;
        return thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out result) ? result : result;
    }

    /// <summary>
    /// 数据转换为int类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static int ObjectToInt(this object thisValue, int errorValue)
    {
        return thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out int result) ? result : errorValue;
    }

    /// <summary>
    /// 数据转换为long类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static long ObjectToLong(this object thisValue)
    {
        long result = 0;
        if (thisValue == null)
            return 0;
        return thisValue != null && thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out result) ? result : result;
    }

    /// <summary>
    /// 数据转换为long类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static long ObjectToLong(this object thisValue, long errorValue)
    {
        return thisValue != null && thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out long result) ? result : errorValue;
    }

    /// <summary>
    /// 数据转换为Double类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static double ObjectToDouble(this object thisValue)
    {
        return thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out double result) ? result : 0.0;
    }

    /// <summary>
    /// 数据转换为Double类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static double ObjectToDouble(this object thisValue, double errorValue)
    {
        return thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out double result) ? result : errorValue;
    }

    /// <summary>
    /// 数据转换为Float类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static float ObjectToFloat(this object thisValue)
    {
        return thisValue != null && thisValue != DBNull.Value && float.TryParse(thisValue.ToString(), out float result) ? result : 0;
    }

    /// <summary>
    /// 数据转换为Float类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static float ObjectToFloat(this object thisValue, float errorValue)
    {
        return thisValue != null && thisValue != DBNull.Value && float.TryParse(thisValue.ToString(), out float result) ? result : errorValue;
    }

    /// <summary>
    /// 数据转换为String类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static string ObjectToString(this object thisValue)
    {
        return thisValue != null ? thisValue.ToString().Trim() : "";
    }

    /// <summary>
    /// 数据转换为String类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static string ObjectToString(this object thisValue, string errorValue)
    {
        return thisValue != null ? thisValue.ToString().Trim() : errorValue;
    }

    /// <summary>
    /// 数据转换为Decimal类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static Decimal ObjectToDecimal(this object thisValue)
    {
        return thisValue != null && thisValue != DBNull.Value && Decimal.TryParse(thisValue.ToString(), out decimal result) ? result : Decimal.Zero;
    }

    /// <summary>
    /// 数据转换为Decimal类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static Decimal ObjectToDecimal(this object thisValue, Decimal errorValue)
    {
        return thisValue != null && thisValue != DBNull.Value && Decimal.TryParse(thisValue.ToString(), out decimal result) ? result : errorValue;
    }

    /// <summary>
    /// 数据转换为DateTime类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static DateTime ObjectToDate(this object thisValue)
    {
        DateTime result = DateTime.MinValue;
        if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out result))
            result = Convert.ToDateTime(thisValue);
        return result;
    }

    /// <summary>
    /// 数据转换为DateTime类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static DateTime ObjectToDate(this object thisValue, DateTime errorValue)
    {
        return thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out DateTime result) ? result : errorValue;
    }

    /// <summary>
    /// 数据转换为bool类型
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static bool ObjectToBool(this object thisValue)
    {
        bool result = false;
        return thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out result) ? result : result;
    }

    public static decimal DecimalToMound(this decimal thisValue, int num)
    {
        return Math.Round(thisValue, num, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 安全转换为枚举，转换失败返回null
    /// </summary>
    public static T? ToEnumOrNull<T>(this object value) where T : struct, Enum
    {
        if (value == null) return null;

        // 如果是整数
        if (value is int intValue)
        {
            if (Enum.IsDefined(typeof(T), intValue))
            {
                return (T)Enum.ToObject(typeof(T), intValue);
            }
        }
        // 如果是 JsonElement（System.Text.Json 反序列化 object 类型时）
        else if (value is JsonElement jsonElement)
        {
            try
            {
                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    var intVal = jsonElement.GetInt32();
                    if (Enum.IsDefined(typeof(T), intVal))
                    {
                        return (T)Enum.ToObject(typeof(T), intVal);
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    if (Enum.TryParse<T>(jsonElement.GetString(), true, out var result))
                    {
                        return result;
                    }
                }
            }
            catch
            {
                // 转换失败返回null
                return null;
            }
        }
        // 如果是字符串
        else if (value is string stringValue)
        {
            if (Enum.TryParse<T>(stringValue, true, out var result))
            {
                return result;
            }
        }
        // 其他类型尝试转换
        else
        {
            try
            {
                var intVal = Convert.ToInt32(value);
                if (Enum.IsDefined(typeof(T), intVal))
                {
                    return (T)Enum.ToObject(typeof(T), intVal);
                }
            }
            catch
            {
                // 转换失败返回null
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// 将文件路径转换为OSS的Key
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <returns></returns>
    public static string ToOssKey(this string fileUrl)
    {
        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out Uri uri))
        {
            return Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        }
        return fileUrl;
    }

    /// <summary>
    /// 将字符串首字母转换为小写（驼峰命名）
    /// </summary>
    /// <param name="value">要转换的字符串</param>
    /// <returns>首字母小写的字符串</returns>
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        if (value.Length == 1)
            return value.ToLowerInvariant();
        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
}
