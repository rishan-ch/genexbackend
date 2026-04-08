using GeneX_Backend.Modules.Products.DTOs;
using GeneX_Backend.Modules.Products.Interfaces;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Users.Interfaces;
using System.Security.Claims;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IUserService _userService;

        public ProductController(IProductService productService, IUserService userService)
        {
            _productService = productService;
            _userService = userService;
        }

        [HttpPost("add-product")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AddProduct([FromForm] AddProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Validation failed", errors = ModelState });

            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });
                if (await _userService.isValidAdmin(UserId))
                    return Unauthorized(new { success = false, message = "You are not authorized to alter with products" });
                var added = await _productService.AddProduct(dto);

                return added
                    ? Ok(new { success = true, message = "Product added successfully." })
                    : Conflict(new { success = false, message = "Product could not be added due to invalid data." });
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
            catch (Exception ex)
            {
                Console.WriteLine("ADD PRODUCT ERROR >>> " + ex.ToString()); // or use ILogger
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        [HttpPost("view-products")]
        public async Task<IActionResult> FilterProducts([FromBody] ProductFilterDTO filter)
        {
            try
            {
                var pagedResult = await _productService.FetchAllProductsAsync(filter);

                return Ok(new
                {
                    success = true,
                    data = pagedResult.Items,
                    lowestPrice = pagedResult.lowestValueRange,
                    highestprice = pagedResult.highestValueRange,
                    pagination = new
                    {
                        pageNumber = pagedResult.PageNumber,
                        pageSize = pagedResult.PageSize,
                        totalCount = pagedResult.TotalCount,
                        totalPages = (int)Math.Ceiling((double)pagedResult.TotalCount / pagedResult.PageSize),
                        previousPage = pagedResult.PageNumber > 1 ? pagedResult.PageNumber - 1 : (int?)null,
                        nextPage = pagedResult.PageNumber < (int)Math.Ceiling((double)pagedResult.TotalCount / pagedResult.PageSize)
                                   ? pagedResult.PageNumber + 1 : (int?)null
                    }
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while filtering products.",
                    details = ex.Message
                });
            }
        }


        [HttpGet("get-productById/{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                ViewProductDTO? product = await _productService.FetchProductByIdAsync(id);
                return Ok(new { success = true, data = product });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred during product fetch", details = ex.Message });

            }
        }


        [HttpGet("get-productBySubCatId/{id}")]
        public async Task<IActionResult> GetProductBySubCatId(Guid id)
        {
            try
            {
                List<ViewProductDTO>? product = await _productService.FetchProductBySubCateIdAsync(id);
                return Ok(new { success = true, data = product });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred during product fetch", details = ex.Message });

            }
        }

        [HttpPut("update-product/{id}")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromForm] UpdateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Validation failed", errors = ModelState });

            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });
                if (await _userService.isValidAdmin(UserId))
                    return Unauthorized(new { success = false, message = "You are not authorized to alter with products" });

                var updated = await _productService.UpdateProductAsync(id, dto);
                return updated
                    ? Ok(new { success = true, message = "Product updated successfully." })
                    : StatusCode(500, new { success = false, message = "Product update failed." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", details = ex.Message });
            }
        }



        [HttpDelete("delete-product/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });
                if (await _userService.isValidAdmin(UserId))
                    return Unauthorized(new { success = false, message = "You are not authorized to alter with products" });

                var deleted = await _productService.DeleteProduct(id);
                if (!deleted)

                    return NotFound(new { success = false, message = "Product not found" });

                return Ok(new { success = true, message = "Product successfully deleted" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occured.", details = ex.Message });
            }
        }


        [HttpGet("hotdeals/{count}")]
        public async Task<IActionResult> FetchHotdeals([FromRoute] int count)
        {
            try
            {
                List<ViewProductDTO> newProducts = await _productService.GetHotDealsProducts(count);

                return Ok(new { success = true, message = newProducts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occured.", details = ex.Message });
            }
        }

        [HttpGet("topRated/{count}")]
        public async Task<IActionResult> FetchTopRated([FromRoute] int count)
        {
            try
            {
                List<ViewProductDTO> newProducts = await _productService.GetTopRatedProducts(count);

                return Ok(new { success = true, message = newProducts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occured.", details = ex.Message });
            }
        }
    }
}


