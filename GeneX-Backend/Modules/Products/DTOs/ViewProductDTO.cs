using GeneX_Backend.Modules.Discount.DTOs;

namespace GeneX_Backend.Modules.Products.DTOs
{
    public class ViewProductDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public List<string>? ProductImageUrl { get; set; }
        public decimal ProductUnitPrice { get; set; }
        public Guid? DiscountId { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int ProductQuantity { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public int Sales { get; set; }
        public string ProductStatus { get; set; }
        public double? AvgRating { get; set; }
        public int? ReviewCount { get; set; }
        public bool? Hotdeals { get; set; }
        public List<ViewProductAttributeDTO>? Attributes{ get; set; }
        
    }

}