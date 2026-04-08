using GeneX_Backend.Modules.Coupon.DTOs;

namespace GeneX_Backend.Modules.Coupon.Interface
{
    public interface ICouponService
    {
        Task AddCoupon(AddCouponDTO addCouponDTO);
        Task<List<ViewCouponDTO>> ViewValidCoupons();
        Task<List<ViewCouponDTO>> ViewAllCoupons();
        Task UpdateCoupon(UpdateCouponDTO updateCouponDTO, Guid CouponId);
        Task<bool> VerifyCoupon(string couponCode, Guid UserId);
    }
}