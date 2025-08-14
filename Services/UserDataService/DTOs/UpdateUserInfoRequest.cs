namespace UserDataService.DTOs;

public class UpdateUserInfoRequest
{
    public string? Username { get; set; }
    
    public string? Bio { get; set; }
    
    public string? Gender { get; set; }
    
    public DateTime Birthday { get; set; }
    
    public string? Location { get; set; }
    
    public int Experience { get; set; }
    
    public int Level { get; set; }
}