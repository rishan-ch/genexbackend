using System.Security.Claims;
using GeneX_Backend.Modules.Category.DTOs;
using GeneX_Backend.Modules.Category.Interface;
using GeneX_Backend.Modules.Users.Interfaces;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("api/subCategory")]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryService _subCatService;
        private readonly IUserService _userService;

        public SubCategoryController(ISubCategoryService subCatService, IUserService userService)
        {
            _subCatService = subCatService;
            _userService = userService;
        }


        [HttpPost("add-subCategory")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateSubCat([FromBody] AddSubCatDTO addSubCatDTO)
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

                await _subCatService.AddNewSubCategory(addSubCatDTO);
                return Ok(new { success = true, message = "A new subcategory has been added" });
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
                return StatusCode(500, new { success = false, message = "An unexpected issue occured" });
            }
        }

        [HttpGet("get-all-subAttributes")]
        public async Task<IActionResult> GetAllSubCat()
        {
            try
            {
                List<ViewSubCatDTO> subCats = await _subCatService.GetAllSubCategory();
                return subCats.Any() ? Ok(new { success = true, message = subCats }) : NotFound(new { success = false, message = "No subcategory attributes were found" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }


        [HttpGet("get-SubCategory/{SubCatID}")]
        public async Task<IActionResult> GetSubCategoryById([FromRoute] Guid SubCatID)
        {
            try
            {
                ViewSubCatDTO? subCat = await _subCatService.GetSubCatByID(SubCatID);
                return subCat != null ? Ok(new { success = true, data = subCat }) : NotFound(new { success = false, message = "No subcategory attributes were found" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }


        [HttpGet("get-by-category/{CategoryId}")]
        public async Task<IActionResult> GetSubCategoriesByCategory([FromRoute] Guid CategoryId)
        {
            try
            {
                List<ViewSubCatDTO>? subCategories = await _subCatService.GetSubCatByCategoryId(CategoryId);

                return Ok(new { success = true, message = subCategories });

            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }



        [HttpPut("/update-subCategory/{subCategoryId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateSubCategory([FromRoute] Guid subCategoryId, [FromBody] UpdateSubCatDTO updateSubCatDTO)
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

                await _subCatService.UpdateSubCategory(subCategoryId, updateSubCatDTO);
                return Ok(new { success = true, message = "Updated" });
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



        [HttpDelete("/delete-subCategory/{subCategoryId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteSubCategory([FromRoute] Guid subCategoryId)
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

                await _subCatService.DeleteSubCategory(subCategoryId);

                return Ok(new { success = true, message = "The subcategory has been deleted" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authorized" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}