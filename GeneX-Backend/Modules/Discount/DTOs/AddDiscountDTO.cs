using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Discount.DTOs
{
    public class AddDiscountDTO
    {
        [Required]
        public int DiscountPercentage { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTill { get; set; } 


    }
}
