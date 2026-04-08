namespace GeneX_Backend.Modules.Discount.DTOs
{
    public class UpdateDiscountDTO
    {

        public string? DiscountName { get; set; }

        public string? DiscountDescription { get; set; }

        public int? DiscountPercentage { get; set; }
    }
}
