using System.Security.Claims;
using GeneX_Backend.Modules.Orders.DTOs;
using GeneX_Backend.Modules.Orders.Interfaces;
using GeneX_Backend.Shared.Enums;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("place-order")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PlaceNewOrder([FromBody] AddOrderDTO addOrderDTO)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid data found. Please try again");

                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _orderService.AddNewOrder(addOrderDTO, UserId);

                return Ok(new { success = true, message = "Order has been placed" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("view-orders")]
        [Authorize]
        public async Task<IActionResult> ViewOrder([FromBody] OrderFilterDTO filters)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (userRole != "Customer")
                {
                    var allOrders = await _orderService.ViewOrders(null, filters);

                    return Ok(new { success = true, message = allOrders });
                }

                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });


                var userOrders = await _orderService.ViewOrders(UserId, filters);

                return Ok(new
                {
                    success = true,
                    message = userOrders.Items,
                    pagination = new
                    {
                        pageNumber = userOrders.PageNumber,
                        pageSize = userOrders.PageSize,
                        totalCount = userOrders.TotalCount,
                        totalPages = (int)Math.Ceiling((double)userOrders.TotalCount / userOrders.PageSize),
                        previousPage = userOrders.PageNumber > 1 ? userOrders.PageNumber - 1 : (int?)null,
                        nextPage = userOrders.PageNumber < (int)Math.Ceiling((double)userOrders.TotalCount / userOrders.PageSize)
                                   ? userOrders.PageNumber + 1 : (int?)null
                    }
                });
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


        [HttpPut("customer/change-status")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateOrderStatusCustomer([FromBody] UpdateStatusDTO updateStatus)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                //users are only allowed to change the status to cancelled
                if (updateStatus.Status == OrderStatus.Cancelled && userRole == "Customer")
                {
                    await _orderService.UpdateOrderStatus(updateStatus);

                    string statusResponse = updateStatus.Status.ToString().ToLower();

                    return Ok(new { success = true, message = $"Order has been {statusResponse}" });
                }
                else
                {
                    return StatusCode(403, new { success = false, message = "Unauthorized request of status change" });
                }

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User is not authorized" });
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

        [HttpPut("admin/change-status")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateOrderStatusAdmin([FromBody] UpdateStatusDTO updateStatus)
        {
            try
            {
                await _orderService.UpdateOrderStatus(updateStatus);

                string statusResponse = updateStatus.Status.ToString().ToLower();

                return Ok(new { success = true, message = $"Order has been {statusResponse}" });


            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
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

        [HttpPut("add-remarks/{orderId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AddRemarks([FromBody] string remarks, [FromRoute] Guid orderId)
        {
            try
            {
                await _orderService.AddRemarks(orderId, remarks);

                return Ok(new { success = true, message = $"Remarks has been added" });

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
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