namespace GeneX_Backend.Modules.Notification.DTOs
{
    public class ViewNotificationDTO
    {
        public required Guid NotificationId { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public required DateTime DateAndTime { get; set; }
        public required bool IsRead { get; set; }
    }
}