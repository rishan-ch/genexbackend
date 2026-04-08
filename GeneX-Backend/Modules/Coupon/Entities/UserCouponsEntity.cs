using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.Users.Entities;

namespace GeneX_Backend.Modules.Coupon.Entities
{
    public class UserCouponEntity
    {
        [Key]
        public Guid UserCouponId { get; set; }
        public required Guid CouponId { get; set; }
        public required Guid UserId { get; set; }
        public required DateTime CouponUseDate { get; set; }

        public UserEntity User { get; set; }
        public CouponEntity Coupon{ get; set; }
    }
}