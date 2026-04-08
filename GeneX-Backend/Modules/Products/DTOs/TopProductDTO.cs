namespace GeneX_Backend.Modules.Products.DTOs
{
    public class TopProductDTO
    {
        public required Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public required int TotalQuantitySold { get; set; }
        public required string Category { get; set; }
        public required int Stock { get; set; }
        public required decimal Price { get; set; }

    }

}