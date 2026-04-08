using System.Security.Claims;
using GeneX_Backend.Modules.Coupon.DTOs;
using GeneX_Backend.Modules.Coupon.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/coupon")]
    public class CouponController : ControllerBase
    {

        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpPost("add-coupon")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AddCoupon([FromForm] AddCouponDTO addCouponDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _couponService.AddCoupon(addCouponDTO);

                    return Ok(new { success = true, message = "New Coupon has been added" });
                }
                return BadRequest(new { success = true, message = "Possible invalid data for model" });
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpPost("verify/{couponCode}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> VerifyCouponCode([FromRoute] string couponCode)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });


                bool isValid = await _couponService.VerifyCoupon(couponCode, UserId);

                if (isValid)
                {
                    return Ok(new { success = true, message = "Coupon is valid" });
                }
                else
                {
                    return Conflict(new { success = false, message = "Invalid coupon code entered" });
                }
                    
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new{ success = false, message = ex.Message });
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


        [HttpGet("view-all-coupon")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ViewAllCoupons()
        {
            try
            {
                List<ViewCouponDTO> coupons = await _couponService.ViewAllCoupons();

                return Ok(new { success = true, message = coupons });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet("view-valid-coupon")]
        public async Task<IActionResult> ViewValidCoupons()
        {
            try
            {
                List<ViewCouponDTO> coupons = await _couponService.ViewValidCoupons();

                return Ok(new { success = true, message = coupons });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpPut("{CouponId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> EditCouponDetails([FromForm] UpdateCouponDTO updateCouponDTO, [FromRoute] Guid CouponId)
        {
            try
            {
                await _couponService.UpdateCoupon(updateCouponDTO, CouponId);

                return Ok(new { success = true, message = "Coupon details has been updated" });
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
