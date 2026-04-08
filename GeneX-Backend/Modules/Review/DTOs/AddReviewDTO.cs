namespace GeneX_Backend.Modules.Review.DTOs
{
    public class AddReviewDTO
    {
        public required Guid ProductId { get; set; }
        public required int StarCount { get; set; }
        public string? Description { get; set; }
    }
}