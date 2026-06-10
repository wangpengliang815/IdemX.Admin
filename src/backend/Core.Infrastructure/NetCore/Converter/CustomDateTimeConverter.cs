namespace Core.Infrastructure.NetCore;

/// <summary>
/// DateTime JSON 格式转换器
/// </summary>
public class CustomDateTimeConverter(string format = "yyyy-MM-dd HH:mm:ss") : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var dateString = reader.GetString();
                    if (DateTime.TryParse(dateString, out var dateTime))
                        return dateTime;
                    throw new JsonException($"无法将 '{dateString}' 转换为 DateTime.");

                case JsonTokenType.Number:
                    // 处理 Unix 时间戳
                    if (reader.TryGetInt64(out var unixTime))
                        return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                    break;

                default:
                    throw new JsonException($"不支持的 TokenType: {reader.TokenType}");
            }

            return reader.GetDateTime();
        }
        catch (Exception ex)
        {
            throw new JsonException($"DateTime 反序列化失败: {ex.Message}", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(format));
    }
}

/// <summary>
/// Nullable DateTime 版本
/// </summary>
public class NullableDateTimeConverter(string format = "yyyy-MM-dd HH:mm:ss") : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var converter = new CustomDateTimeConverter(format);
        return converter.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString(format));
    }
}