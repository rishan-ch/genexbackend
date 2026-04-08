namespace GeneX_Backend.Modules.Users.DTOs
{
    public class ResetPayload
    {
        public required string Email { get; set; }
        public required string Token{ get; set; }
    }
}
