using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Products.DTOs
{
    public class UpdateProductDTO
    {

        [Required(ErrorMessage = "A Product Name is required.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product description required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product description minimum lenght not reached!")]
        public string ProductDescription { get; set; }

        public int ProductQuantity { get; set; }

        public decimal ProductUnitPrice { get; set; }

        public Guid? DiscountId { get; set; }

        public List<IFormFile>? ProductImage { get; set; }
        public bool Hotdeals{ get; set; }

        public string Attributes { get; set; }



    }
}
