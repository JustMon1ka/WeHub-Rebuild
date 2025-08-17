using Microsoft.EntityFrameworkCore;

namespace PostService.DTOs;

[Keyless]
public class PostScore
{
    public long PostId { get; set; }
    public double OracleScore { get; set; }
}