using System.Security.Claims;
using GeneX_Backend.Modules.Notification.DTOs;
using GeneX_Backend.Modules.Notification.Service;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ShowAllNotification()
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                List<ViewNotificationDTO>? notifs = await _notificationService.ViewNotifications( UserId);

                return Ok(new { success = true, message = notifs });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        [HttpPut("{notificationId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateNotification([FromRoute] Guid notificationId)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _notificationService.MarkAsRead(notificationId, UserId);

                return Ok(new { success = true, message = "Notification marked as read" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpDelete("{notificationId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveNotifications([FromRoute] Guid notificationId)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _notificationService.DeleteNotification(notificationId, UserId);

                return Ok(new { success = true, message = "Notification has been removed" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}