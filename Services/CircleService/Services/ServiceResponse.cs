namespace CircleService.Services;

/// <summary>
/// 用于封装Service层操作结果的通用响应类。
/// </summary>
public class ServiceResponse
{
    /// <summary>
    /// 表示操作是否成功。
    /// </summary>
    public bool Success { get; protected set; }

    /// <summary>
    /// 包含操作失败时的错误信息。
    /// </summary>
    public string? ErrorMessage { get; protected set; }

    /// <summary>
    /// 创建一个表示成功的响应。
    /// </summary>
    public static ServiceResponse Succeed() => new ServiceResponse { Success = true };

    /// <summary>
    /// 创建一个表示失败的响应。
    /// </summary>
    /// <param name="errorMessage">错误信息。</param>
    public static ServiceResponse Fail(string errorMessage) => new ServiceResponse { Success = false, ErrorMessage = errorMessage };
} 