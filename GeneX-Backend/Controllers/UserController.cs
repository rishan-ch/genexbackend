using GeneX_Backend.Modules.Users.DTOs;
using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Modules.Users.Interfaces;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using System.Net;

namespace GeneX_Backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserEntity> _logger;
        private readonly IJWTService _jwtService;

        private readonly string _frontendUrl;

        public UserController(
            IUserService userService,
            ILogger<UserEntity> logger,
            IJWTService jwtService,
            IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _jwtService = jwtService;
            _frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
        }

        [HttpPost("all")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> FetchAll([FromBody] ViewUserFilterDto filter)
        {
            try
            {
                var users = await _userService.GetAllUsers(filter);

                return Ok(new { success = true, message = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterNewUser([FromForm] UserRegistrationDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {

                var result = await _userService.AddNewUser(userDTO, "Customer");
                if (result.Succeeded)
                {
                    return Ok(new { success = true, message = "A new user has been registered to the system" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Errors.Select(e => e.Description) });
                }
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering new user");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }


        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UserUpdateDTO userUpdateDTO)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });


                await _userService.UpdateUser(userUpdateDTO, UserId);

                return Ok(new { success = true, message = "User information updated" });
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


        [HttpGet("authenticateEmail")]
        public async Task<IActionResult> EmailConfirmation([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                bool confirmed = await _userService.ConfirmUserEmail(email, token);
                if (confirmed)
                {
                    return Redirect($"{_frontendUrl}/pages/confirm-email?status=success");
                }
                else
                {
                    return Redirect($"{_frontendUrl}/pages/confirm-email?status=failed");
                }
            }
            catch (NotFoundException ex)
            {
                return Redirect($"{_frontendUrl}/pages/confirm-email?status=failed&message={WebUtility.UrlEncode(ex.Message)}");
            }
            catch (Exception ex)
            {
                return Redirect($"{_frontendUrl}/pages/confirm-email?status=failed&message={WebUtility.UrlEncode(ex.Message)}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var (token, userId, role, email) = await _userService.LoginAsync(loginDto);
                if (token == null)
                {
                    return Unauthorized(new { success = false, message = "Login failed. Required token could not be generated" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Login Successful",
                    token,
                    userId,
                    role,
                    email
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login.");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });


                var userProfile = await _userService.GetUserProfileByIdAsync(UserId);

                if (userProfile == null)
                {
                    return NotFound(new { success = false, message = "User profile not found." });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve profile.");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the profile." });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotDto)
        {
            try
            {
                await _userService.ForgotPassword(forgotDto);
                return Ok(new
                {
                    Success = true,
                    Message = "Password reset email sent successfully"
                });
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


        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changeDto)
        {
            try
            {
                //extract user id from the jwt token
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null) return Unauthorized("Unauthorized user");

                //converting user id into Guid from string
                if (!Guid.TryParse(userIdString, out Guid UserId))
                    return BadRequest(new { success = false, message = "Invalid User id" });

                await _userService.VerifyAndChangePwd(UserId, changeDto);

                return Ok(new { success = true, message = "The password has been changed" });

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



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            try
            {
                var (succeeded, errors) = await _userService.ResetPassword(resetDto);

                if (!succeeded)
                {
                    return Redirect($"{_frontendUrl}/pages/change-password?status=failed");
                }

                return Redirect($"{_frontendUrl}/pages/change-password?status=success");
            }
            catch (NotFoundException ex)
            {
                return Redirect($"{_frontendUrl}/pages/change-password?status=failed&message={WebUtility.UrlEncode(ex.Message)}");
            }
            catch (Exception ex)
            {
                return Redirect($"{_frontendUrl}/pages/change-password?status=failed&message={WebUtility.UrlEncode(ex.Message)}");
            }
        }


        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDto.IdToken);
                var user = await _userService.GetOrCreateGoogleUserAsync(payload);
                var roles = await _userService.GetUserRolesAsync(user);
                var jwt = _jwtService.GenerateToken(user, roles);

                return Ok(new { success = true, token = jwt });
            }
            catch (InvalidJwtException ex)
            {
                return Unauthorized(new { success = false, message = "Invalid Google token", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error", details = ex.Message });
            }
        }

        [HttpDelete("ban/{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> BanUser([FromRoute] Guid userId)
        {
            try
            {
                await _userService.BanUser(userId);

                return Ok(new { success = true, message = "The user has been banned" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidActionException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("delete/{userId}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
        {
            try
            {
                await _userService.DeleteUser(userId);

                return Ok(new { success = true, message = "The user has been deleted permenently" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidActionException ex)
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
