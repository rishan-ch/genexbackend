namespace GeneX_Backend.Modules.Coupon.DTOs
{
    public class ViewCouponDTO
    {
        public required Guid CouponId { get; set; }
        public required string CouponCode { get; set; }
        public required string CouponName { get; set; }
        public string? CouponImageUrl { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required decimal DiscountPercent { get; set; }
        public int? UseCount{ get; set; }
    }
}