using System.Transactions;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Infrastructure.Notification;
using GeneX_Backend.Modules.Notification.DTOs;
using GeneX_Backend.Modules.Notification.Entities;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Notification.Service
{
    public class NotificationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext appDbContext, IHubContext<NotificationHub> hubContext)
        {
            _appDbContext = appDbContext;
            _hubContext = hubContext;
        }

        // Remove expired notifications using UTC comparison
        private async Task RemoveExpiredNotifications()
        {
            var expiredNotifs = await _appDbContext.Notifications
                .Where(n => n.ExpiraryTime <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredNotifs.Any())
            {
                _appDbContext.Notifications.RemoveRange(expiredNotifs);
                await _appDbContext.SaveChangesAsync();
            }
        }

        // Get notifications and convert DateTime to local time for API
        private async Task<List<ViewNotificationDTO>> GetActiveNotifications(Guid userId)
        {
            return await _appDbContext.Notifications
                .Where(n => n.UserId == userId || n.UserId == null)
                .OrderByDescending(n => n.DateAndTime)
                .Select(no => new ViewNotificationDTO
                {
                    NotificationId = no.NotificationId,
                    Title = no.Title,
                    Message = no.Message,
                    DateAndTime = no.DateAndTime.ToLocalTime(), // local time with offset
                    IsRead = no.IsRead
                })
                .ToListAsync();
        }

        public async Task<List<ViewNotificationDTO>> ViewNotifications(Guid userId)
        {
            await RemoveExpiredNotifications();
            return await GetActiveNotifications(userId);
        }

        public async Task MarkAsRead(Guid notificationId, Guid userId)
        {
            var noti = await _appDbContext.Notifications.FirstOrDefaultAsync(
                n => n.NotificationId == notificationId && n.UserId == userId
            );

            if (noti == null)
                throw new NotFoundException("The required notification doesn't exist");

            noti.IsRead = true;
            _appDbContext.Notifications.Update(noti);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteNotification(Guid notificationId, Guid userId)
        {
            var noti = await _appDbContext.Notifications.FirstOrDefaultAsync(
                n => n.NotificationId == notificationId && n.UserId == userId
            );

            if (noti == null)
                throw new NotFoundException("The required notification doesn't exist");

            _appDbContext.Notifications.Remove(noti);
            await _appDbContext.SaveChangesAsync();
        }

        // Create notification and send to a specific user
        public async Task CreateAndSendAsync(Guid userId, string title, string message)
        {
            try
            {
                var notification = new NotificationEntity
                {
                    NotificationId = Guid.NewGuid(),
                    DateAndTime = DateTime.UtcNow, // store UTC
                    IsRead = false,
                    Message = message,
                    Title = title,
                    UserId = userId,
                    ExpiraryTime = DateTime.UtcNow.AddHours(24) // optional expiry
                };

                _appDbContext.Notifications.Add(notification);
                await _appDbContext.SaveChangesAsync();

                var notificationDto = new ViewNotificationDTO
                {
                    NotificationId = notification.NotificationId,
                    Title = notification.Title,
                    Message = notification.Message,
                    DateAndTime = notification.DateAndTime.ToLocalTime(),
                    IsRead = notification.IsRead
                };

                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", notificationDto);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send notification", ex);
            }
        }

        // Send notification to all users
        public async Task SendToAllUsersAsync(string title, string message)
        {
            var notification = new NotificationEntity
            {
                NotificationId = Guid.NewGuid(),
                DateAndTime = DateTime.UtcNow,
                IsRead = false,
                Message = message,
                Title = title,
                ExpiraryTime = DateTime.UtcNow.AddHours(24)
            };

            _appDbContext.Notifications.Add(notification);

            var notificationDto = new ViewNotificationDTO
            {
                NotificationId = notification.NotificationId,
                Title = title,
                Message = message,
                DateAndTime = notification.DateAndTime.ToLocalTime(),
                IsRead = false
            };

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notificationDto);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
