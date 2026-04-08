namespace GeneX_Backend.Modules.Discount.DTOs
{
    public class ViewDiscountDTO
    {
        public Guid DiscountId { get; set; }
        public int DiscountPercentage { get; set; }
        public string ValidFrom { get; set; }
        public string ValidTill { get; set; }

    }
}
