namespace UserDataService.DTOs;

public class UpdateUserInfoRequest
{
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