using System.ComponentModel.DataAnnotations;

namespace FollowService.DTOs
{
    public class CreateFollowDto
    {
        [Required(ErrorMessage = "被关注用户ID是必填项")]
        public int FolloweeId { get; set; }
    }
}