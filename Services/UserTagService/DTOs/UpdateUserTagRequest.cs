namespace UserTagService.DTOs;

public class UpdateUserTagRequest
{
    public List<int> Tags { get; set; } = new();
}