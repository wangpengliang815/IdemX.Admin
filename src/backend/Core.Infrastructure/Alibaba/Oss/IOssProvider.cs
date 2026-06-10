namespace Core.Infrastructure.Alibaba;

public interface IOssProvider
{
    /// <summary>
    /// 上传文件到OSS
    /// </summary>
    /// <param name="memStream">文件流</param>
    /// <param name="fileName">对象 Key</param>
    /// <param name="contentType">Content-Type，未传时默认 image/jpeg</param>
    /// <returns>文件URL</returns>
    string PutObject(MemoryStream memStream, string fileName, string contentType = "image/jpeg");

    /// <summary>
    /// 同桶内拷贝对象，返回与 PutObject 一致风格的可访问 URL（不含 query）
    /// </summary>
    string CopyObjectWithinBucket(string sourceKey, string destinationKey);

    /// <summary>
    /// 从公网 URL 解析桶内对象 Key（路径样式 URL 会去掉首段 Bucket 名，与 PutObject/Copy 所用 Key 一致）
    /// </summary>
    string PublicUrlToObjectKey(string publicUrl);

    /// <summary>
    /// 从OSS删除文件
    /// </summary>
    /// <param name="key">文件键</param>
    /// <returns>是否删除成功</returns>
    bool DeleteObject(string key);

    /// <summary>
    /// 按前缀列举对象，仅删除 LastModified 早于 cutoffUtc 的键，返回删除条数。
    /// </summary>
    int DeleteObjectsByPrefixOlderThan(string prefix, DateTime cutoffUtc);

    /// <summary>
    /// 获取OSS图片URL
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>图片URL</returns>
    string GetImgUrl(string fileName, float width = 100, float height = 100);
}