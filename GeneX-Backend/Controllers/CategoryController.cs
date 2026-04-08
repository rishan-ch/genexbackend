using System.Security.Claims;
using System.Threading.Tasks;
using GeneX_Backend.Modules.Category.DTOs;
using GeneX_Backend.Modules.Category.Interface;
using GeneX_Backend.Modules.Users.Interfaces;
using GeneX_Backend.Modules.Users.Services;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [Route("/api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        public CategoryController(ICategoryService categoryService, IUserService userService)
        {
            _categoryService = categoryService;
            _userService = userService;
        }



        [HttpPost("add-category")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateCategory(AddCategoryDTO categoryDTO)
        {
            try
            {

                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });
                if (await _userService.isValidAdmin(UserId))
                    return Unauthorized(new { success = false, message = "You are not authorized to alter with categories" });


                await _categoryService.AddNewCategory(categoryDTO.Categoryname);

                return Ok(new { success = true, message = "New category has been registered" });

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
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }




        [HttpGet("get-all-categories")]
        public IActionResult GetAllCategories()
        {
            try
            {
                var categories = _categoryService.GetAllCategory();
                return Ok(new { success = true, data = categories });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
            }
        }




        [HttpGet("get-by-id/{id}")]
        public IActionResult GetCategoryById(Guid id)
        {
            try
            {
                var category = _categoryService.GetCategoryById(id);
                return Ok(new { success = true, data = category });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
            }
        }




        [HttpPut("update-category/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] AddCategoryDTO categoryDTO)
        {
            try
            {

                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });
                if (await _userService.isValidAdmin(UserId))
                    return Unauthorized(new { success = false, message = "You are not authorized to alter with categories" });

                await _categoryService.UpdateCategory(id, categoryDTO.Categoryname);
                return Ok(new { success = true, message = "Category updated successfully." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }




        [HttpDelete("delete-category/{categoryId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteCategory(Guid categoryId)
        {
            try
            {
       var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });
                if (await _userService.isValidAdmin(UserId))
                    return Unauthorized(new { success = false, message = "You are not authorized to alter with categories" });

                await _categoryService.DeleteCategory(categoryId);
                return Ok(new { success = true, message = "Category and all its related items i.e. subcategory and product has been removed" });
            }
            catch (KeyNotFoundException e)
            {
                return Conflict(new { success = false, message = e.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }

    }
}