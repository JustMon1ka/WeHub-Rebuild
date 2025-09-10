using System.Net;
using System.Net.Mail;
using UserAuthService.Data;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Services;
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly EmailCodeStorage _codes;

    public EmailService(IConfiguration config)
    {
        _config = config;
        _codes = new EmailCodeStorage(config);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtp = _config["Email:SmtpServer"];
        var port = int.Parse(_config["Email:Port"]!);
        var sender = _config["Email:SenderEmail"];
        var password = _config["Email:SenderPassword"];

        using var client = new SmtpClient(smtp, port)
        {
            Credentials = new NetworkCredential(sender, password),
            EnableSsl = bool.Parse(_config["Email:EnableSsl"]!)
        };

        body += $"\n有效时间为：{_config["Email:ExpiresMinutes"] ?? "10"}分钟";
        body += $"\n如果不是您本人操作，请忽略此邮件。";
        body += $"\n\n此邮件由{_config["Email:SenderName"] ?? "WeHub"}发送。";
        var message = new MailMessage(sender, to, subject, body);
        await client.SendMailAsync(message);
    }
    
    public void SaveCode(string email, string code)
    {
        _codes.SaveCode(email, code);
    }

    public bool ValidateCode(string email, string code)
    {
        return _codes.ValidateCode(email, code);
    }
}