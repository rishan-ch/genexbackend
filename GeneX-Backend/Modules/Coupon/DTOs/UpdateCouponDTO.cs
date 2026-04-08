namespace GeneX_Backend.Modules.Coupon.DTOs
{
    public class UpdateCouponDTO
    {
        public required string CouponCode { get; set; }
        public required string CouponName { get; set; }
        public IFormFile? CouponImage { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required decimal DiscountPercent { get; set; }
    }
}