namespace NoticeService.DTOs
{
    public class LikeResponseDto
    {
        public List<LikeNotificationDto> Unread { get; set; }
        public ReadLikesDto Read { get; set; }
    }

    public class ReadLikesDto
    {
        public int Total { get; set; }
        public List<LikeNotificationDto> Items { get; set; }
    }
}