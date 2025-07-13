namespace UserAuthService.DTOs;

public class LoginRequest
{
    public string Identifier { get; set; } = null!; // 可为用户名、邮箱或手机号
    public string Password { get; set; } = null!;
}