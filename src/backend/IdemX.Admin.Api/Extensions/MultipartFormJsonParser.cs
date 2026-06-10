namespace IdemX.Admin.Api.Extensions;

/// <summary>
/// multipart 表单字段 request（JSON 字符串）反序列化
/// </summary>
public static class MultipartFormJsonParser
{
    /// <summary>
    /// 解析 form 字段 request 为指定 DTO
    /// </summary>
    public static bool TryDeserialize<T>(string json, JsonSerializerOptions options, out T value, out string error)
    {
        value = default!;
        error = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "请求参数不能为空";
            return false;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<T>(json, options);
            if (parsed is null)
            {
                error = "请求参数无效";
                return false;
            }

            value = parsed;
            return true;
        }
        catch (JsonException)
        {
            error = "请求参数格式错误";
            return false;
        }
    }

    /// <summary>
    /// 表单未传文件字段时模型绑定为 null，规范为空列表供 Service 使用
    /// </summary>
    public static List<IFormFile> NormalizeFileList(List<IFormFile> files) =>
        files ?? [];
}
