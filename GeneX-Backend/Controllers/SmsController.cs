using System;
using System.Threading.Tasks;
using GeneX_Backend.Modules.SMS.DTO;
using GeneX_Backend.Modules.SMS.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [Route("/api/sms")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly ISparrowSmsService _smsService;

        public SmsController(ISparrowSmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpPost("send")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SendSms([FromBody] SendSMSDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.To))
                    return BadRequest(new { success = false, message = "Recipient phone number(s) is required." });

                if (string.IsNullOrWhiteSpace(request.Text))
                    return BadRequest(new { success = false, message = "Message text is required." });

                var apiResponse = _smsService.SendSmsAsync(request);

                // You can parse apiResponse if you want to check success/failure from Sparrow's response,
                // but for now just assume success if no exception thrown

                return Ok(new
                {
                    success = true,
                    message = "SMS sent successfully.",
                    data = apiResponse
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // This is from your service wrapping HTTP call exceptions
                return StatusCode(500, new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}
