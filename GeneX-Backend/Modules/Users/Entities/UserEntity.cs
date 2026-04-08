using Microsoft.AspNetCore.Identity;

namespace GeneX_Backend.Modules.Users.Entities
{
    public class UserEntity : IdentityUser<Guid>
    {
        public string? ProfileImageUrl { get; set; }
        public required string FirstName { get; set; }
        public string? LastName { get; set; }
        public required string Address { get; set; }
        public required bool isDeleted { get; set; }
    }
}

