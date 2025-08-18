namespace UserDataService.DTOs;

public class UserInfoResponse
{
    public int UserId { get; set; }
    
    public string? Username { get; set; }
    
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int Status { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    public string? Bio { get; set; }
    
    public string? Gender { get; set; }
    
    public DateTime Birthday { get; set; }
    
    public string? Location { get; set; }
    
    public int Experience { get; set; }
    
    public int Level { get; set; }
    
    public string? Nickname { get; set; }
    
    
    public string? ProfileUrl { get; set; }
}