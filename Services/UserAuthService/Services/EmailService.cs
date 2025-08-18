using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Services;
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ConcurrentDictionary<string, string> _codes = new();

    public EmailService(IConfiguration config)
    {
        _config = config;
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

        var message = new MailMessage(sender, to, subject, body);
        await client.SendMailAsync(message);
    }
    
    public void SaveCode(string eamil, string code)
    {
        _codes[eamil] = code;
    }

    public bool ValidateCode(string email, string code)
    {
        return _codes.TryGetValue(email, out var stored) && stored == code;
    }
}