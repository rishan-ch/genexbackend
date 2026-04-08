using System.Linq.Expressions;
using System.Security.Claims;
using GeneX_Backend.Modules.BillingInfo.DTO;
using GeneX_Backend.Modules.BillingInfo.Interface;
using GeneX_Backend.Modules.Cart.DTOs;
using GeneX_Backend.Modules.Cart.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/billingInfo")]
    public class BillingInfoController : ControllerBase
    {

        private readonly IBillingInfoService _billingService;

        public BillingInfoController(IBillingInfoService billingService)
        {
            _billingService = billingService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddBillingInfo([FromBody] AddBillingDTO addBillingDTO)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid data entered. Please try again");
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _billingService.AddBillingInfo(addBillingDTO, UserId);

                return Ok(new { sucess = true, message = "New billing information saved" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{BillingInfoId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ViewBillingInfo([FromRoute] Guid BillingInfoId)
        {
            try
            {
                ViewBillingDTO billingInfo = await _billingService.ViewBillingInfo(BillingInfoId);

                return Ok(new { success = true, message = billingInfo });
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

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ViewAllBillingInfo()
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                List<ViewBillingDTO>? billingInfos = await _billingService.ViewAllBillingInfo(UserId);

                return Ok(new { success = true, message = billingInfos });
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

        [HttpDelete("{BillingInfoId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteBillingInfo([FromRoute] Guid BillingInfoId)
        {
            try
            {
                await _billingService.DeleteBillingInfo(BillingInfoId);

                return Ok(new { success = true, message = "Billing info removed" });
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

        [HttpPut]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> EditBillingInfo([FromBody] AddBillingDTO addBillingDTO, Guid BillingInfoId)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid data entered. Please try again");
                await _billingService.UpdateBillingInfo(addBillingDTO, BillingInfoId);

                return Ok(new { success = true, message = "Billing info has been updated" });
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
