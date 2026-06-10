namespace Core.Infrastructure.Alibaba;

/// <summary>
/// 阿里云Oss存储服务提供商
/// </summary>
/// <param name="options"></param>
public class AliyunOssProvider(IOptions<AliyunOptions> options) : IOssProvider
{
    private readonly OssOptions ossOptions = options.Value.Oss;
    private readonly Lazy<OssClient> lazyOssClient = new(() => new OssClient(options.Value.Oss.Endpoint, options.Value.Oss.AccessKeyId, options.Value.Oss.AccessKeySecret));

    private OssClient OssClient => lazyOssClient.Value;

    /// <summary>
    /// 上传文件到OSS
    /// </summary>
    /// <param name="memStream">文件流</param>
    /// <param name="fileName">对象 Key</param>
    /// <param name="contentType">Content-Type</param>
    /// <returns>文件URL</returns>
    public string PutObject(MemoryStream memStream, string fileName, string contentType = "image/jpeg")
    {
        ObjectMetadata objectMetadata = new();
        objectMetadata.AddHeader("Content-Type", contentType);

        OssClient.PutObject(ossOptions.BucketName, fileName, memStream, objectMetadata);
        return BuildPublicUrlWithoutQuery(fileName);
    }

    /// <summary>
    /// 同桶内拷贝对象
    /// </summary>
    public string CopyObjectWithinBucket(string sourceKey, string destinationKey)
    {
        // 阿里云 SDK：CopyObjectRequest(源桶, 源 Key, 目标桶, 目标 Key)，顺序不可对调
        var request = new CopyObjectRequest(ossOptions.BucketName, sourceKey, ossOptions.BucketName, destinationKey);
        OssClient.CopyObject(request);
        return BuildPublicUrlWithoutQuery(destinationKey);
    }

    /// <summary>
    /// 公网 URL → 桶内对象 Key；路径样式访问（域名后第一段为 Bucket）时去掉该段，与 SDK 使用的 Key 一致。
    /// </summary>
    public string PublicUrlToObjectKey(string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
            return string.Empty;

        var key = publicUrl.ToOssKey();
        var bucketPrefix = $"{ossOptions.BucketName}/";
        if (key.Length > bucketPrefix.Length && key.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
            return key[bucketPrefix.Length..];

        return key;
    }

    private string BuildPublicUrlWithoutQuery(string fileName)
    {
        var result = OssClient.GeneratePresignedUri(ossOptions.BucketName, fileName).ToString();

        if (!string.IsNullOrEmpty(result) && result.Contains('?', StringComparison.Ordinal))
            result = result[..result.IndexOf('?', StringComparison.Ordinal)];

        return result;
    }

    /// <summary>
    /// 从OSS删除文件
    /// </summary>
    /// <param name="key">文件键</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteObject(string key)
    {
        OssClient.DeleteObject(ossOptions.BucketName, key);
        return true;
    }

    /// <summary>
    /// 分页列举前缀，仅删除 LastModified 早于 cutoffUtc 的对象。
    /// </summary>
    public int DeleteObjectsByPrefixOlderThan(string prefix, DateTime cutoffUtc)
    {
        var deleted = 0;
        string marker = null;
        while (true)
        {
            var request = new ListObjectsRequest(ossOptions.BucketName)
            {
                Prefix = prefix,
                MaxKeys = 1000,
            };
            if (!string.IsNullOrEmpty(marker))
                request.Marker = marker;

            var result = OssClient.ListObjects(request);
            foreach (var s in result.ObjectSummaries)
            {
                if (string.IsNullOrEmpty(s.Key))
                    continue;
                if (ToLastModifiedUtc(s.LastModified) >= cutoffUtc)
                    continue;

                OssClient.DeleteObject(ossOptions.BucketName, s.Key);
                deleted++;
            }

            if (!result.IsTruncated || string.IsNullOrEmpty(result.NextMarker))
                break;

            marker = result.NextMarker;
        }

        return deleted;
    }

    private static DateTime ToLastModifiedUtc(DateTime lastModified)
    {
        if (lastModified.Kind is DateTimeKind.Utc)
            return lastModified;
        if (lastModified.Kind is DateTimeKind.Local)
            return lastModified.ToUniversalTime();

        return DateTime.SpecifyKind(lastModified, DateTimeKind.Utc);
    }

    /// <summary>
    /// 获取OSS图片URL
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>图片URL</returns>
    public string GetImgUrl(string fileName, float width = 100, float height = 100)
    {
        var process = $"image/resize,m_fixed,w_{width},h_{height}";
        var req = new GeneratePresignedUriRequest(ossOptions.BucketName, fileName, SignHttpMethod.Get)
        {
            Expiration = DateTime.Now.AddHours(1),
            Process = process
        };
        var result = OssClient.GeneratePresignedUri(req).ToString();
        return result;
    }
}