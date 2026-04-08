using GeneX_Backend.Modules.Discount.DTOs;
using GeneX_Backend.Modules.Discount.Interface;
using GeneX_Backend.Modules.Email;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/discount")]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpPost("add-discount")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AddDiscount([FromBody] AddDiscountDTO discountDto)
        {

            try
            {
                bool isAdded = await _discountService.AddDiscountAsync(discountDto);


                return isAdded
                    ? Ok(new { success = true, message = "Discount added successfully." })
                    : Conflict(new { success = false, message = "Discount could not be added due to invalid data." });

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("get-all-discounts")]
        public async Task<IActionResult> GetAllDiscount([FromQuery] ViewDiscountFilterDto filter)
        {
            try
            {
                var discounts = await _discountService.GetAllDiscount(filter);
                return Ok(new { success = true, data = discounts });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred" });
            }
        }



        [HttpGet("get-discount-by-id/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            try
            {
                var discount = await _discountService.GetDiscountById(id);
                return Ok(new { success = true, data = discount });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred" });
            }
        }

        [HttpDelete("deleteDiscount/{id}")]
        [Authorize(Roles = "SuperAdmin")]

        public async Task<IActionResult> DeleteDiscount(Guid id)
        {
            try
            {
                bool isDeleted = await _discountService.DeleteDiscountAsync(id);
                return isDeleted
                    ? Ok(new { success = true, message = "Discount deleted successfully." })
                    : NotFound(new { success = false, message = "Discount not found." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
            }
        }

    }
}
