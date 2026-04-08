using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.Users.Entities;

namespace GeneX_Backend.Modules.Notification.Entities
{
    public class NotificationEntity
    {
        [Key]
        public Guid NotificationId { get; set; }
        public Guid? UserId { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public required DateTime DateAndTime { get; set; }
        public required bool IsRead{ get; set; }
        public DateTime? ExpiraryTime{get;set;}
        public UserEntity? User { get; set; }
    }
}