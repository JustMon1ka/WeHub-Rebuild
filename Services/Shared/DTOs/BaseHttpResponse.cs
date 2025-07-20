namespace DTOs;

public class BaseHttpResponse<T>
{
    public int Code { get; set; }       // 状态码
    public string Msg { get; set; }     // 提示信息
    public T? Data { get; set; }         // 泛型数据
    
    public BaseHttpResponse(int code, string msg, T? data)
    {
        Code = code;
        Msg = msg;
        Data = data;
    }

    // 快速创建成功响应
    public static BaseHttpResponse<T> Success(T data, string msg = "OK")
    {
        return new BaseHttpResponse<T>(200, msg, data);
    }

    // 快速创建失败响应
    public static BaseHttpResponse<T> Fail(int code, string msg)
    {
        return new BaseHttpResponse<T>(code, msg, default!);
    }
}