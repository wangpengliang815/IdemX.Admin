namespace Core.Model.Shared;

/// <summary>
/// 统一接口返回值模型（泛型：用于返回具体数据）
/// </summary>
/// <typeparam name="T">Data 的类型</typeparam>
public class CustomApiResponse<T>
{
    /// <summary>
    /// 业务状态码（0=成功，非0=失败）
    /// </summary>
    public int Code { get; set; } = 0;

    /// <summary>
    /// 提示信息
    /// </summary>
    public string Msg { get; set; } = string.Empty;

    /// <summary>
    /// 数据对象
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// 数据总数（分页用）
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 统一构建方法
    /// </summary>
    public static CustomApiResponse<T> Result(int code, string msg, T data = default, int total = 0)
    {
        return new CustomApiResponse<T>
        {
            Code = code,
            Msg = msg,
            Data = data,
            Total = total
        };
    }

    /// <summary>
    /// 成功快捷方法（使用默认成功码 0）
    /// </summary>
    public static CustomApiResponse<T> Ok(string msg, T data = default, int count = 0)
    {
        return Result(0, msg, data, count);
    }

    /// <summary>
    /// 失败快捷方法（使用默认失败码 1）
    /// </summary>
    public static CustomApiResponse<T> Fail(string msg, T data = default)
    {
        return Result(1, msg, data, 0);
    }
}

/// <summary>
/// 统一接口返回值模型（非泛型：用于只返回状态消息，不返回具体 Data）
/// </summary>
public class CustomApiResponse : CustomApiResponse<object>
{
    /// <summary>
    /// 成功快捷方法（默认码 0）
    /// </summary>
    public new static CustomApiResponse Ok(string msg, object data = null, int count = 0)
    {
        return new CustomApiResponse { Code = 0, Msg = msg, Data = data, Total = count };
    }

    /// <summary>
    /// 失败快捷方法（默认码 1）
    /// </summary>
    public new static CustomApiResponse Fail(string msg, object data = null)
    {
        return new CustomApiResponse { Code = 1, Msg = msg, Data = data };
    }
}
