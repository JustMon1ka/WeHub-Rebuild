namespace DTOs;

public class BaseHttpResponse<T>
{
    public int Code { get; set; }       // 状态码
    public string Msg { get; set; }     // 提示信息
    public T? Data { get; set; }         // 泛型数据 (允许为null)
    
    public BaseHttpResponse(int code, string msg, T? data)
    {
        Code = code;
        Msg = msg;
        Data = data;
    }

    // 快速创建成功响应
    public static BaseHttpResponse<T> Success(T? data, string msg = "OK")
    {
        return new BaseHttpResponse<T>(200, msg, data);
    }

    // 快速创建失败响应
    public static BaseHttpResponse<T> Fail(int code, string msg)
    {
        // 使用 default! 来为泛型 T 提供一个默认值 (通常是 null)
        return new BaseHttpResponse<T>(code, msg, default!);
    }
}

// 为了方便在Controller中调用Fail，可以提供一个非泛型的静态类
public static class BaseHttpResponse
{
    public static BaseHttpResponse<object> Fail(int code, string msg)
    {
        return BaseHttpResponse<object>.Fail(code, msg);
    }
}