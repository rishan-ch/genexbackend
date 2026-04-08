using System.Text;
using GeneX_Backend.Infrastructure.CloudinaryService;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Coupon.DTOs;
using GeneX_Backend.Modules.Coupon.Entities;
using GeneX_Backend.Modules.Coupon.Interface;
using GeneX_Backend.Modules.Notification.Service;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Coupon.Service
{

    public class CouponService : ICouponService
    {

        private readonly AppDbContext _appDbContext;
        private readonly CloudinaryService _cloudinaryService;
        private readonly NotificationService _notificationService;


        public CouponService(AppDbContext appDbContext, CloudinaryService cloudinaryService, NotificationService notificationService)
        {
            _appDbContext = appDbContext;
            _cloudinaryService = cloudinaryService;
            _notificationService = notificationService;
        }



        public async Task AddCoupon(AddCouponDTO addCouponDTO)
        {

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                //if admin did not enter a custom code then generate new one
                var code = addCouponDTO.CouponCode == null ? await GenerateCode() : addCouponDTO.CouponCode;

                if (addCouponDTO.CouponCode != null && !await IsUnique(code))
                    throw new AlreadyExistsException("The coupon code already exists");

                string? imageUrl = null;
                if (addCouponDTO.CouponImage != null)
                {
                    //upload image to cloudinary
                    imageUrl = await _cloudinaryService.UploadImageAsync(addCouponDTO.CouponImage);
                }

                _appDbContext.Coupons.Add(new CouponEntity()
                {
                    CouponCode = code,
                    CouponName = addCouponDTO.CouponName,
                    CouponImageUrl = imageUrl,
                    DiscountPercent = addCouponDTO.DiscountPercent,
                    EndDate = addCouponDTO.EndDate,
                    StartDate = addCouponDTO.StartDate
                });
                await _appDbContext.SaveChangesAsync();

                await _notificationService.SendToAllUsersAsync("New coupons available", "New coupons have been introduced to the system. Do check it out");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


        }


        //generates the code for coupon
        public async Task<string> GenerateCode()
        {
            string CouponCode = "";
            bool isValid = true;

            //creates new code until it is unique
            while (isValid)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random();
                var result = new StringBuilder(8);

                for (int i = 0; i < 8; i++)
                {
                    result.Append(chars[random.Next(chars.Length)]);
                }

                if (await IsUnique(result.ToString()))
                {
                    isValid = false;
                    CouponCode = result.ToString();
                }
            }
            return CouponCode;
        }


        //checks if code is unique
        public async Task<bool> IsUnique(string Code)
        {
            CouponEntity? coupon = await _appDbContext.Coupons.FirstOrDefaultAsync(c => c.CouponCode == Code);
            return coupon == null;
        }

        //view all coupon codes for - admin
        public async Task<List<ViewCouponDTO>> ViewAllCoupons()
        {
            return await _appDbContext.Coupons.Select(
                c => new ViewCouponDTO()
                {
                    CouponId = c.CouponId,
                    CouponCode = c.CouponCode,
                    CouponName = c.CouponName,
                    CouponImageUrl = c.CouponImageUrl,
                    DiscountPercent = c.DiscountPercent,
                    EndDate = c.EndDate,
                    StartDate = c.StartDate,
                    UseCount = _appDbContext.Orders.Count(o => o.CouponId == c.CouponId)
                }
            ).ToListAsync();
        }

        public async Task<List<ViewCouponDTO>> ViewValidCoupons()
        {
            var now = DateTime.UtcNow;

            return await _appDbContext.Coupons
                .Where(c => c.StartDate <= now && c.EndDate >= now)
                .Select(c => new ViewCouponDTO
                {
                    CouponId = c.CouponId,
                    CouponCode = c.CouponCode,
                    CouponName = c.CouponName,
                    CouponImageUrl = c.CouponImageUrl,
                    DiscountPercent = c.DiscountPercent,
                    EndDate = c.EndDate,
                    StartDate = c.StartDate,
                    UseCount = _appDbContext.Orders.Count(o => o.CouponId == c.CouponId)
                })
                .ToListAsync();
        }


        public async Task UpdateCoupon(UpdateCouponDTO updateCouponDTO, Guid CouponId)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                CouponEntity? coupon = await _appDbContext.Coupons.FirstOrDefaultAsync(c => c.CouponId == CouponId);
                if (coupon == null) throw new NotFoundException("The coupon doen't exists");

                string? imageUrl = null;
                if (updateCouponDTO.CouponImage != null)
                {
                    //upload image to cloudinary
                    imageUrl = await _cloudinaryService.UploadImageAsync(updateCouponDTO.CouponImage);
                }

                coupon.CouponCode = updateCouponDTO.CouponCode;
                coupon.CouponImageUrl = imageUrl;
                coupon.CouponName = updateCouponDTO.CouponName;
                coupon.DiscountPercent = updateCouponDTO.DiscountPercent;
                coupon.EndDate = updateCouponDTO.EndDate;
                coupon.StartDate = updateCouponDTO.StartDate;

                _appDbContext.Coupons.Update(coupon);
                await _appDbContext.SaveChangesAsync();
                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task<bool> VerifyCoupon(string couponCode, Guid UserId)
        {
            //checks if the coupon code exists
            CouponEntity? coupon = await _appDbContext.Coupons.FirstOrDefaultAsync(c => c.CouponCode == couponCode);

            if (coupon == null) throw new NotFoundException("The required coupon doesn't exists");

            //checks time validity of the coupon
            if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
                throw new NotFoundException("The coupon is not valid at this time");


            //checks the coupons used by the user
            UserCouponEntity? userCoupon = await _appDbContext.UserCoupons.FirstOrDefaultAsync(uc => uc.UserId == UserId && uc.Coupon.CouponCode == couponCode);

            if (userCoupon != null)
                throw new ArgumentException("You have already used the coupon");

            //return true if null
            return userCoupon == null;
        }
    }
}