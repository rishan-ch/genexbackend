using GeneX_Backend.Modules.Category.Entities;
using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Products.DTOs
{
    public class AddProductDTO
    {

        public Guid SubCategoryId { get; set; }

        [Required(ErrorMessage = "A Product Name is required.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product description required.")]
        public string ProductDescription { get; set; }

        public int ProductQuantity { get; set; }

        public decimal ProductUnitPrice { get; set; }

        public Guid? DiscountId { get; set; } 

        public List<IFormFile>? ProductImage { get; set; }
        public required bool HotDeals { get; set; }

        public string Attributes { get; set; }


    }
}
