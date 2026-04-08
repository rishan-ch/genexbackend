using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Modules.Users.Interfaces;
using GeneX_Backend.Modules.Users.DTOs;
using Microsoft.AspNetCore.Identity;
using GeneX_Backend.Shared.Exceptions;
using System.Net;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using GeneX_Backend.Modules.Email;
using System.Text;
using System.Text.Json;
using System.Formats.Asn1;
using GeneX_Backend.Modules.Products.DTOs;
using GeneX_Backend.Infrastructure.CloudinaryService;


namespace GeneX_Backend.Modules.Users.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<UserEntity> _userManager;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IJWTService _jwtService;
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly IEmailService _emailService;
        private readonly string _baseUrl;
        private readonly CloudinaryService _cloudinaryService;
        private readonly string _frontendUrl;

        public UserService(AppDbContext appDbContext,
            UserManager<UserEntity> userManager,
            SignInManager<UserEntity> signInManager,
            IJWTService jwtService,
            RoleManager<RoleEntity> roleManager,
            IEmailService emailService,
            IConfiguration configuration,
            CloudinaryService cloudinaryService)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _roleManager = roleManager;
            _emailService = emailService;
            _baseUrl = configuration["Domain:BaseUrl"];
            _cloudinaryService = cloudinaryService;
            _frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
        }

        public async Task<IdentityResult> AddNewUser(UserRegistrationDTO userDTO, string role)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            string ? imageUrl = userDTO.ProfileImage != null
                ? await _cloudinaryService.UploadImageAsync(userDTO.ProfileImage)
                : null;
            try
            {
                UserEntity newUser = new UserEntity
                {
                    ProfileImageUrl = imageUrl,
                    FirstName = userDTO.Firstname,
                    LastName = userDTO.Lastname,
                    UserName = userDTO.Email!.Split("@")[0],
                    Email = userDTO.Email,
                    PhoneNumber = userDTO.PhoneNumber,
                    Address = userDTO.Address,
                    EmailConfirmed = false,
                    isDeleted = false
                };

                //email, phonenumber duplicacy check
                await VerifyDetails(userDTO);

                //adds the user to the db
                var result = await _userManager.CreateAsync(newUser, userDTO.Password);

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new RoleEntity { Name = role });
                }

                await _userManager.AddToRoleAsync(newUser, role);

                //send email only if user is registered
                if (result.Succeeded)
                {
                    await SendConfirmationEmail(newUser, userDTO);
                }

                await transaction.CommitAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }



        }

        //user email confimration
        public async Task<bool> ConfirmUserEmail(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException($"The user with {email} email does not exist in the system");

            string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            var confirmResult = await _userManager.ConfirmEmailAsync(user, decodedToken);

            return confirmResult.Succeeded;
        }



        //sends email with email confirmation link
        public async Task SendConfirmationEmail(UserEntity user, UserRegistrationDTO userDTO)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var callbackUrl = $"{_baseUrl}api/user/authenticateEmail?token={base64Token}&email={WebUtility.UrlEncode(user.Email)}";

            await _emailService.SendConfirmationEmail(user.FirstName, callbackUrl, user.Email);
        }




        //email, phonenumber duplicacy check
        private async Task VerifyDetails(UserRegistrationDTO userDTO)
        {
            var existingUser = await _userManager.FindByEmailAsync(userDTO.Email);
            if (existingUser != null)
            {
                throw new AlreadyExistsException("Email already exists.");
            }

            var phoneExists = _appDbContext.Users.Any(u => u.PhoneNumber == userDTO.PhoneNumber);
            if (phoneExists)
            {
                throw new AlreadyExistsException("Phone number already exists.");
            }
        }

        //Login Service
        public async Task<(string? Token, Guid UserId, string? RoleId, string email)> LoginAsync(UserLoginDTO loginDTO)
        {
            UserEntity? user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (user.isDeleted)
                throw new NotFoundException("Your account has been disabled. Please contact our customer sevice.");

            //checks if email is confirmed
            if (!user.EmailConfirmed) throw new UnauthorizedAccessException("Confirm your email first.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            string? role = null;
            if (roles.Count > 0)
            {
                var roleDetails = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roles[0]);
                role = roleDetails?.Name;
            }

            string email = user.Email;

            var token = _jwtService.GenerateToken(user, roles);
            return (token, user.Id, role, email);
        }


        //Get User Profile
        public async Task<UserProfileDTO?> GetUserProfileByIdAsync(Guid id)
        {
            UserEntity? user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new NotFoundException("The user doesn't exist in the system.");

            if (user.isDeleted)
                throw new NotFoundException("Your account has been disabled. Please contact our customer sevice.");

            var roles = await _userManager.GetRolesAsync(user);
            return new UserProfileDTO
            {
                UserId = user.Id,
                ProfileImageUrl = user.ProfileImageUrl,
                Firstname = user.FirstName,
                Lastname = user.LastName,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault(),
                isBanned = user.isDeleted
            };
        }

        public async Task ForgotPassword(ForgotPasswordDto forgotDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotDto.Email);
            if (user == null)
                throw new NotFoundException("User not found");

            if (!user.EmailConfirmed)
                throw new UnauthorizedAccessException("Please confirm your email before changing your password");

            if (user.isDeleted)
                throw new NotFoundException("Your account has been disabled. Please contact our customer sevice.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode email + token as JSON
            var payload = new
            {
                Email = user.Email,
                Token = token
            };  

            string jsonPayload = JsonSerializer.Serialize(payload);
            string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload));

            var resetUrl = $"{_frontendUrl}/pages/reset-password?data={base64Payload}";

            await _emailService.SendPassowordResetLink(user.FirstName, resetUrl, user.Email);
        }


        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPassword(ResetPasswordDto dto)
        {
            string decodedJson = Encoding.UTF8.GetString(Convert.FromBase64String(dto.Data));

            var payload = JsonSerializer.Deserialize<ResetPayload>(decodedJson);

            if (payload == null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Token))
                throw new UnauthorizedAccessException("Invalid token");

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
                throw new NotFoundException("User not found");

            var result = await _userManager.ResetPasswordAsync(user, payload.Token, dto.NewPassword);

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }



        public async Task<UserEntity> GetOrCreateGoogleUserAsync(GoogleJsonWebSignature.Payload payload)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new UserEntity
                    {
                        FirstName = payload.GivenName,
                        LastName = payload.FamilyName,
                        Email = payload.Email,
                        UserName = payload.Email,
                        EmailConfirmed = true,
                        Address = string.Empty, // or set to a default value as appropriate
                        isDeleted = false
                    };
                    await _userManager.CreateAsync(user);
                }
                await transaction.CommitAsync();
                return user;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task<IList<string>> GetUserRolesAsync(UserEntity user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<UserEntity> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new NotFoundException($"User with ID {id} not found.");
            }
            return user;
        }

        public async Task UpdateUser(UserUpdateDTO userUpdateDTO, Guid UserId)
        {
            // Fetch the user
            UserEntity? fetchedUser = await GetUserByIdAsync(UserId);
        
            if (fetchedUser == null)
                throw new NotFoundException("User doesn't exist in the system");
        
            // Update profile image if provided
            if (userUpdateDTO.ProfileImage != null && userUpdateDTO.ProfileImage.Length > 0)
            {
                string newImageUrl = await _cloudinaryService.UploadImageAsync(userUpdateDTO.ProfileImage);
                fetchedUser.ProfileImageUrl = newImageUrl;
            }
        
            // Update other fields only if they are not null
            if (!string.IsNullOrEmpty(userUpdateDTO.Firstname))
                fetchedUser.FirstName = userUpdateDTO.Firstname;
        
            if (!string.IsNullOrEmpty(userUpdateDTO.Lastname))
                fetchedUser.LastName = userUpdateDTO.Lastname;
        
            if (!string.IsNullOrEmpty(userUpdateDTO.Address))
                fetchedUser.Address = userUpdateDTO.Address;
        
            if (!string.IsNullOrEmpty(userUpdateDTO.PhoneNumber))
                fetchedUser.PhoneNumber = userUpdateDTO.PhoneNumber;
        
            // Save changes
            _appDbContext.Users.Update(fetchedUser);
            await _appDbContext.SaveChangesAsync();
        }


        //fetches user details of every users registered as customers
        public async Task<PagedResult<UserProfileDTO>> GetAllUsers(ViewUserFilterDto filter)
        {
            var query = from u in _appDbContext.Users
                        join ur in _appDbContext.UserRoles on u.Id equals ur.UserId
                        join r in _appDbContext.Roles on ur.RoleId equals r.Id
                        where r.Name == "Customer"
                        select new UserProfileDTO
                        {
                            UserId = u.Id,
                            Firstname = u.FirstName,
                            Lastname = u.LastName,
                            Email = u.Email,
                            Address = u.Address,
                            PhoneNumber = u.PhoneNumber,
                            Role = r.Name,
                            isBanned = u.isDeleted
                        };

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((filter.pageNumber - 1) * filter.pageSize)
                .Take(filter.pageSize)
                .ToListAsync();

            return new PagedResult<UserProfileDTO>
            {
                Items = users,
                PageNumber = filter.pageNumber,
                PageSize = filter.pageSize,
                TotalCount = totalCount
            };
        }



        //soft deletes the user
        public async Task BanUser(Guid userId)
        {
            UserEntity? user = await _appDbContext.Users.FirstOrDefaultAsync(
                u => u.Id == userId
            );

            if (user == null)
                throw new NotFoundException("The user doesn't exists in the system");

            if (user.isDeleted)
                throw new InvalidActionException("The user has already been banned");

            user.isDeleted = true;

            _appDbContext.Users.Update(user);

            await _appDbContext.SaveChangesAsync();
        }

        //permanently removes the user if no orders are made
        public async Task DeleteUser(Guid userId)
        {
            UserEntity? user = await _appDbContext.Users
                .FirstOrDefaultAsync(
                    u => u.Id == userId
                );

            if (user == null)
                throw new NotFoundException("The user doesn't exists in the system");

            _appDbContext.Users.Remove(user);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<bool> isValidAdmin(Guid userId)
        {
            UserEntity? user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return user?.UserName == "shrawan.tamrakar";
        }

        public async Task VerifyAndChangePwd(Guid UserId, ChangePasswordDTO changeDto)
        {
            UserEntity? user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == UserId);

            if (user == null)
                throw new NotFoundException("User not found");

            bool result = await _userManager.CheckPasswordAsync(user, changeDto.OldPassword);

            if (result)
            {
                await _userManager.ChangePasswordAsync(user, changeDto.OldPassword, changeDto.NewPassword);
            }
            else
            {
                throw new InvalidOperationException("Old password is incorrect");
            }

            

        }
    }
}
