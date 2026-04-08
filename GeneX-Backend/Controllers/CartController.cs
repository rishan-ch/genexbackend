using System.Security.Claims;
using GeneX_Backend.Modules.Cart.DTOs;
using GeneX_Backend.Modules.Cart.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/cart")]
    public class CartController : ControllerBase
    {

        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("add-to-cart")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddItemsToCart([FromBody] AddToCartDTO addToCartDTO)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _cartService.AddProductToCart(addToCartDTO, UserId);

                return Ok(new { success = true, message = "A product has been added to the cart" });
            }
            catch (InvalidQuantityException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (NotFoundException e)
            {
                return NotFound(new { success = false, message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }

        }

        [HttpPost("increase-quantity")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> IncreaseQuanitty(ChangeQtyDTO changeQtyDTO)
        {
            try
            {
                changeQtyDTO.Quantity = 1;
                await _cartService.ChangeQuantity(changeQtyDTO);
                return Ok(new { success = true, message = "Quantity increased" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }

        [HttpPost("decrease-quantity")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DecreaseQuantity(ChangeQtyDTO changeQtyDTO)
        {
            try
            {
                changeQtyDTO.Quantity = -1;
                await _cartService.ChangeQuantity(changeQtyDTO);
                return Ok(new { success = true, message = "Quantity decreased" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }

        [HttpGet("cart-items")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ViewAllCartItems()
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                List<ViewCartDTO>? cartItems = await _cartService.ViewCart(UserId);

                if (cartItems == null)
                {
                    return Ok(new { succss = true, Message = "No items found in the cart" });
                }
                else
                {
                    return Ok(new { succss = true, Message = cartItems });
                }


            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }

        [HttpDelete("remove-item")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveItem([FromBody] DelItemDTO delItemDTO)
        {
            try
            {
                //removes only one item from the cart
                await _cartService.DeleteCartItem(delItemDTO);
                return Ok(new { success = true, message = "Item removed from your cart" });
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

        [HttpDelete("remove-all")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveAllItems()
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _cartService.DeleteAllItems(UserId);
                return Ok(new { success = true, message = "Cleared cart items" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}