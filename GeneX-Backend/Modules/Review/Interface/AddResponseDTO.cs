namespace GeneX_Backend.Modules.Review.DTOs
{
    public class AddResponseDTO
    {
        public required Guid ReviewId { get; set; }
        public required string Description { get; set; }

    }
}