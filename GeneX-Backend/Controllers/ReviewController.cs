using System.Security.Claims;
using GeneX_Backend.Modules.Review.DTOs;
using GeneX_Backend.Modules.Review.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("api/review")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("add-review")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDTO addReview)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                //checks the starcount
                if (addReview.StarCount < 1 || addReview.StarCount > 5)
                    return BadRequest(new { success = false, message = "Invalid ratings" });

                await _reviewService.AddReview(addReview, UserId);

                return Ok(new { success = true, message = "Review has been added" });

            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("get-reviews/{productId}")]
        public async Task<IActionResult> GetProductReviews([FromRoute] Guid productId)
        {
            try
            {
                ViewReviewDTO reviews = await _reviewService.ViewReviewByProduct(productId);

                return Ok(new { success = true, message = reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("add-reply")]
        [Authorize]
        public async Task<IActionResult> ReviewResponseAdd([FromBody] AddResponseDTO addResponseDTO)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                await _reviewService.AddReviewResponse(addResponseDTO, UserId, userRole);

                return Ok(new { success = true, message = "Response has been recorded" });

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


