namespace GeneX_Backend.Modules.Review.DTOs
{
    public class AdminReplyDTO
    {
        public required Guid ReviewId{ get; set; }
        public required string AdminReply { get; set; }
    }
}