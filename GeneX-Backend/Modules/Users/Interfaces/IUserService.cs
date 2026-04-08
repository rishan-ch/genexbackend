using GeneX_Backend.Modules.Users.DTOs;
using GeneX_Backend.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Google.Apis.Auth;
using GeneX_Backend.Modules.Products.DTOs;

namespace GeneX_Backend.Modules.Users.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserProfileDTO>> GetAllUsers(ViewUserFilterDto filter);
        Task BanUser(Guid userId);
        Task DeleteUser(Guid userId);
        Task<IdentityResult> AddNewUser(UserRegistrationDTO userDTO, string role);
        Task UpdateUser(UserUpdateDTO userUpdateDTO, Guid UserId);
        Task<bool> ConfirmUserEmail(string email, string token);
        Task SendConfirmationEmail(UserEntity user, UserRegistrationDTO userDTO);

        Task<(string? Token, Guid UserId, string? RoleId, string email)> LoginAsync(UserLoginDTO loginDTO);

        Task<UserProfileDTO?> GetUserProfileByIdAsync(Guid id);
        Task ForgotPassword(ForgotPasswordDto forgotDto);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPassword(ResetPasswordDto dto);
        Task VerifyAndChangePwd(Guid UserId, ChangePasswordDTO changeDto);


        Task<UserEntity> GetOrCreateGoogleUserAsync(GoogleJsonWebSignature.Payload payload);
        Task<IList<string>> GetUserRolesAsync(UserEntity user);

        Task<bool> isValidAdmin(Guid userId);

    }
}
