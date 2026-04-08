using GeneX_Backend.Modules.Banner.Entity;
using GeneX_Backend.Modules.BillingInfo.Entity;
using GeneX_Backend.Modules.Cart.Entities;
using GeneX_Backend.Modules.Category.Entities;
using GeneX_Backend.Modules.Coupon.Entities;
using GeneX_Backend.Modules.Discount.Entities;
using GeneX_Backend.Modules.Enquiry.Entities;
using GeneX_Backend.Modules.Notification.Entities;
using GeneX_Backend.Modules.Orders.Entities;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Modules.WishList.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<UserEntity, RoleEntity, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Define DbSets
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<SubCategoryEntity> SubCategories { get; set; }
        public DbSet<SubCategoryAttributeEntity> SubCategoryAttributes { get; set; }
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<ProductAttributeEntity> ProductAttributes { get; set; }
        public DbSet<DiscountEntity> Discounts { get; set; }
        public DbSet<CartEntity> Carts { get; set; }
        public DbSet<CartItemEntity> CartItems { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }
        public DbSet<ReviewEntity> Reviews { get; set; }
        public DbSet<CouponEntity> Coupons { get; set; }
        public DbSet<UserCouponEntity> UserCoupons { get; set; }
        public DbSet<BillingInfoEntity> BillingInfos { get; set; }
        public DbSet<ReviewResponseEntity> ReviewResponses { get; set; }
        public DbSet<WishlistEntity> Wishlists { get; set; }
        public DbSet<BannerEntity> Banners { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }
        public DbSet<ProductImagesEntity> ProductImages { get; set; }
        public DbSet<EnquiryEntity> Enquiries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category -> SubCategory
            modelBuilder.Entity<SubCategoryEntity>()
                .HasOne(s => s.CategoryEntity)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subcategory -> SubCategoryAttribute
            modelBuilder.Entity<SubCategoryAttributeEntity>()
                .HasOne(sca => sca.SubCategoryEntity)
                .WithMany(c => c.SubCategoryAttributeEntities)
                .HasForeignKey(c => c.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart -> User
            modelBuilder.Entity<CartEntity>()
                .HasOne(cart => cart.User)
                .WithOne()
                .HasForeignKey<CartEntity>(cart => cart.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart -> CartItem
            modelBuilder.Entity<CartItemEntity>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> Product
            modelBuilder.Entity<CartItemEntity>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubCategory -> Product
            modelBuilder.Entity<ProductEntity>()
                .HasOne(p => p.SubCategoryEntity)
                .WithMany(s => s.Products)
                .HasForeignKey(s => s.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> ProductAttribute
            modelBuilder.Entity<ProductAttributeEntity>()
               .HasOne(pa => pa.ProductEntity)
               .WithMany(p => p.ProductAttributes)
               .HasForeignKey(p => p.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

            // SubCategoryAttribute -> ProductAttribute
            modelBuilder.Entity<ProductAttributeEntity>()
                .HasOne(pa => pa.SubCategoryAttribute)
                .WithMany(sca => sca.ProductAttributes)
                .HasForeignKey(pa => pa.SubCategoryAttributeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> Discount
            modelBuilder.Entity<ProductEntity>()
                .HasOne(p => p.DiscountEntity)
                .WithMany(d => d.Products)
                .HasForeignKey(p => p.DiscountId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order -> User (Cascade so orders are removed if user is deleted)
            modelBuilder.Entity<OrderEntity>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order ↔ OrderItem
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Product
            modelBuilder.Entity<OrderItemEntity>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> User (Cascade so reviews are removed if user is deleted)
            modelBuilder.Entity<ReviewEntity>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review -> Product
            modelBuilder.Entity<ReviewEntity>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserCoupon -> Coupon
            modelBuilder.Entity<UserCouponEntity>()
                .HasOne(uc => uc.Coupon)
                .WithMany()
                .HasForeignKey(uc => uc.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            // Coupon -> Order
            modelBuilder.Entity<OrderEntity>()
                .HasOne(ord => ord.Coupon)
                .WithMany()
                .HasForeignKey(ord => ord.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserCoupon -> User (Cascade so coupons are removed if user is deleted)
            modelBuilder.Entity<UserCouponEntity>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCouponEntity>()
                .HasIndex(uc => new { uc.UserId, uc.CouponId })
                .IsUnique();

            modelBuilder.Entity<CouponEntity>()
                .HasIndex(c => c.CouponCode)
                .IsUnique();

            // BillingInfo -> User (Cascade so billing info is removed if user is deleted)
            modelBuilder.Entity<BillingInfoEntity>()
                .HasOne(bi => bi.User)
                .WithMany()
                .HasForeignKey(bi => bi.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order -> BillingInfo
            modelBuilder.Entity<OrderEntity>()
                .HasOne(o => o.BillingInfo)
                .WithMany()
                .HasForeignKey(o => o.BillingInfoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReviewResponse -> Review
            modelBuilder.Entity<ReviewResponseEntity>()
                .HasOne(r => r.Review)
                .WithMany()
                .HasForeignKey(r => r.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wishlist -> User
            modelBuilder.Entity<WishlistEntity>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wishlist -> Product
            modelBuilder.Entity<WishlistEntity>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevents duplicate wishlist entries
            modelBuilder.Entity<WishlistEntity>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique();

            modelBuilder.Entity<ProductImagesEntity>()
                .HasOne(p => p.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
