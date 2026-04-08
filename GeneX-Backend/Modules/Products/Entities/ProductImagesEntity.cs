using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Products.Entities
{
    public class ProductImagesEntity
    {
        [Key]
        public Guid ProductImageId { get; set; }
        public required Guid ProductId { get; set; }
        public required string ProductImageUrl { get; set; }
        
        public ProductEntity Product{ get; set; }
    }
}