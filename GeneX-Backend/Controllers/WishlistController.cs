using System.Security.Claims;
using GeneX_Backend.Modules.WishList.DTOs;
using GeneX_Backend.Modules.WishList.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/wishlist")]
    public class WishListController : ControllerBase
    {

        private readonly IWishlistService _wishlistService;

        public WishListController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddWishlist([FromBody] AddWishlistDTO addWishlistDTO)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _wishlistService.AddNewItem(addWishlistDTO, UserId);

                return Ok(new { success = true, message = "Product added to wishlist" });
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ViewWishlists()
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                List<ViewWishlistDTO>? wishlist = await _wishlistService.ViewAllWishlist(UserId);

                return Ok(new { success = true, message = wishlist });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        


        [HttpDelete("{wishlistId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveWishlist([FromRoute] Guid wishlistId)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _wishlistService.RemoveItem(wishlistId, UserId);

                return Ok(new { success = true, message = "Product removed from wishlist" });
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
