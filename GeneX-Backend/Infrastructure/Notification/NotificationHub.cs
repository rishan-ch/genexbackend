using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using GeneX_Backend.Modules.Notification.Service;
using GeneX_Backend.Modules.Notification.DTOs;
using Microsoft.Extensions.Logging;

namespace GeneX_Backend.Infrastructure.Notification
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly NotificationService _notificationService;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(NotificationService notificationService, ILogger<NotificationHub> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task SendNotification(string userId, string title, string message)
        {
            _logger.LogInformation("SendNotification invoked for user {UserId} by {CallerId}", userId, Context.UserIdentifier);
            if (Context.UserIdentifier != userId)
            {
                _logger.LogWarning("Unauthorized: User ID mismatch. Expected {UserId}, got {CallerId}", userId, Context.UserIdentifier);
                throw new HubException("Unauthorized: User ID mismatch");
            }

            await _notificationService.CreateAndSendAsync(Guid.Parse(userId), title, message);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {UserId}", Context.UserIdentifier);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {UserId}, Reason: {Reason}", Context.UserIdentifier, exception?.Message);
            await base.OnDisconnectedAsync(exception);
        }
    }
}