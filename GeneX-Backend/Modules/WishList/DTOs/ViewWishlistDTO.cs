namespace GeneX_Backend.Modules.WishList.DTOs
{
    public class ViewWishlistDTO
    {
        public required Guid WishlistId{ get; set; }
        public required Guid ProductId { get; set; }
        public string? ImageUrl { get; set; }
        public required string ProductName { get; set; }
        public required decimal ProductPrice{ get; set; }
    }
}