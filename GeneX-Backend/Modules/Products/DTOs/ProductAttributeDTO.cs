using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Products.DTOs
{
    public class ProductAttributeDTO
    {
        public Guid SubCategoryAttributeId { get; set; }

        public string SubCategoryAttributeName { get; set; }

        [Required]
        public string ProductAttributeValue { get; set; }
    }
}
