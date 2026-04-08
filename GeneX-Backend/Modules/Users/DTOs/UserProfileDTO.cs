namespace GeneX_Backend.Modules.Users.DTOs
{
    public class UserProfileDTO
    {
        public required Guid UserId { get; set; }
         public string ProfileImageUrl { get; set; }
        public required string Firstname { get; set; }
        public string? Lastname { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Role { get; set; }
        public required bool isBanned{ get; set; }
    }
}
