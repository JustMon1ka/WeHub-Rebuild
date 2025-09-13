using System.ComponentModel.DataAnnotations;

namespace NoticeService.DTOs
{
    public class CreateLikeNotificationDto
    {
        [Required]
        public int LikerId { get; set; } // 点赞者ID (用户A)

        [Required]
        public int TargetUserId { get; set; } // 被点赞内容的作者ID (用户B)

        [Required]
        public int TargetId { get; set; } // 被点赞的内容ID (帖子ID或评论ID)

        [Required]
        [StringLength(10)]
        public string TargetType { get; set; } = string.Empty; // "POST" 或 "COMMENT"

        public int LikeType { get; set; } = 1; // 点赞类型，默认为1
    }
}
