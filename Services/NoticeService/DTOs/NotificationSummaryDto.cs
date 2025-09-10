namespace NoticeService.DTOs
{
    public class NotificationSummaryDto
    {
        public int TotalUnread { get; set; }
        public Dictionary<string, int> UnreadByType { get; set; } // e.g., {"reply": 10, "like": 15, ...}
    }
}