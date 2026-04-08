using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Coupon.Entities
{
    public class CouponEntity
    {
        [Key]
        public Guid CouponId { get; set; }
        public required string CouponCode { get; set; }
        public required string CouponName { get; set; }
        public string? CouponImageUrl{ get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required decimal DiscountPercent{ get; set; }
    }
}