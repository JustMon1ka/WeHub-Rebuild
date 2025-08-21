using System.Collections.Concurrent;

namespace UserAuthService.Data;

public class EmailCodeStorage
{
    private class EmailCode
    {
        public readonly string Code;
        public readonly DateTime CreatedAt;

        public EmailCode(string code)
        {
            Code = code;
            CreatedAt = DateTime.UtcNow;
        }
    }
    private readonly ConcurrentDictionary<string, EmailCode> _codes = new();
    private readonly int _expirationMinutes;
    private readonly Timer _cleanupTimer;

    private void ClearExpiredCodes()
    {
        var now = DateTime.UtcNow;
        foreach (var key in _codes.Keys)
        {
            if (_codes.TryGetValue(key, out var code) &&
                now - code.CreatedAt > TimeSpan.FromMinutes(_expirationMinutes))
            {
                _codes.TryRemove(key, out _);
            }
        }
    }
    
    public EmailCodeStorage(IConfiguration config)
    {
        _expirationMinutes = int.Parse(config["Email:ExpiresMinutes"] ?? "10");
        // Set up a timer to clear expired codes every 5 minutes
        _cleanupTimer = new Timer(_ => ClearExpiredCodes(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }
    
    public bool ValidateCode(string email, string code)
    {
        if (!_codes.TryGetValue(email, out var stored)) return false;
        // Check if the code matches and is within the 10-minute validity period
        return stored.Code == code && DateTime.UtcNow - stored.CreatedAt <= TimeSpan.FromMinutes(10);
    }
    
    public void SaveCode(string email, string code)
    {
        _codes[email] = new EmailCode(code);
    }
}