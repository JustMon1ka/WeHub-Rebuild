namespace UserAuthService.Services.Interfaces;

using UserAuthService.DTOs;
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    void SaveCode(string phone, string code);
    bool ValidateCode(string phone, string code);
}