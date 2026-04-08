using GeneX_Backend.Modules.Products.Entities;

namespace GeneX_Backend.Modules.Cart.DTOs
{
    public class ViewCartDTO
    {
        public required Guid CartItemId { get; set; }
        public required Guid ProductId{ get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }
        public required decimal PriceAfterDiscount{ get; set; }
        public string? ImageUrl { get; set; }
        
    }
}