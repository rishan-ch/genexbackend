namespace GeneX_Backend.Modules.Review.DTOs
{
    public class ViewReviewDTO
    {
        public required decimal Average { get; set; }
        public ICollection<ReviewList>? ReviewList { get; set; } = new List<ReviewList>();

    }

    public class ReviewList
    {
        public required Guid ReviewId { get; set; }
        public required int StarCount { get; set; }
        public required string Username { get; set; }
        public required DateTime ReviewDate { get; set; }
        public string? Description { get; set; }
        
        public List<ResponseList>? Responses { get; set; }
    }

    public class ResponseList
    {
        public required string Username { get; set; }
        public required DateTime ResponseDate { get; set; }
        public required string Description{ get; set; }
        public bool? isAdmin { get; set; }
    }
}