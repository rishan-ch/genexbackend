using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.BillingInfo.Entity;
using GeneX_Backend.Modules.Coupon.Entities;
using GeneX_Backend.Modules.Email;
using GeneX_Backend.Modules.Notification.Service;
using GeneX_Backend.Modules.Orders.DTOs;
using GeneX_Backend.Modules.Orders.Entities;
using GeneX_Backend.Modules.Orders.Interfaces;
using GeneX_Backend.Modules.Products.DTOs;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Shared.Enums;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Orders.Services
{

    public class OrderService : IOrderService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IEmailService _emailService;
        private readonly IInvoiceService _invoiceService;
        private readonly NotificationService _notificationService;

        public OrderService(IInvoiceService invoiceService, AppDbContext appDbContext, IEmailService emailService, NotificationService notificationService)
        {
            _appDbContext = appDbContext;
            _emailService = emailService;
            _invoiceService = invoiceService;
            _notificationService = notificationService;
        }

        public async Task VerifyBillingInfo(Guid UserId, Guid BIllingInfoId)
        {
            try
            {
                BillingInfoEntity? billingInfo = await _appDbContext.BillingInfos.FirstOrDefaultAsync(
                    bi => bi.BillingInfoId == BIllingInfoId && bi.UserId == UserId
                );
                if (billingInfo == null) throw new NotFoundException("The required billing information doesn't exists");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddNewOrder(AddOrderDTO addOrderDTO, Guid UserId)
        {

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {

                await VerifyBillingInfo(UserId, addOrderDTO.BillingInfoId);
                decimal OrderTotal = 0;
                CouponEntity? Coupon = null;

                //gets all the products to add in the order
                var products = await FetchProductsDict(addOrderDTO.OrderedItems);

                //for calculating the order total
                foreach (var item in addOrderDTO.OrderedItems)
                {
                    var product = products[item.ProductId];
                    OrderTotal += product.ProductUnitPrice * item.quantity;
                }

                if (addOrderDTO.CouponCode != null)
                {
                    bool isValid = await ValidateCoupon(UserId, addOrderDTO.CouponCode);
                    if (isValid)
                    {
                        Coupon = await GetCoupon(addOrderDTO.CouponCode);
                    }

                }



                //amount before and after discount will be same as there are no additional logics
                var addedEntity = _appDbContext.Orders.Add(
                        new OrderEntity
                        {
                            OrderDateTime = DateTime.UtcNow,
                            UserId = UserId,
                            AmountBeforeDiscount = OrderTotal,
                            AmountAfterDiscount = Coupon == null ? OrderTotal
                                : OrderTotal - (OrderTotal * Coupon.DiscountPercent / 100),
                            Status = OrderStatus.Placed,
                            PaymentMethod = addOrderDTO.PaymentMode,
                            CouponId = Coupon?.CouponId,
                            BillingInfoId = addOrderDTO.BillingInfoId
                        }
                    );
                //necessary for generating the order id
                await _appDbContext.SaveChangesAsync();

                await AddOrderItems(addedEntity.Entity.OrderId, addOrderDTO.OrderedItems, products);

                if (Coupon != null)
                {
                    await RecordUsedCoupons(UserId, Coupon.CouponId);
                }

                await _appDbContext.SaveChangesAsync();

                await _notificationService.CreateAndSendAsync(UserId, "Order placed", "Your order has been placed. You will be getting the confirmation call soon.");

                var invoicePdf = await _invoiceService.GenerateInvoicePdfAsync(addedEntity.Entity.OrderId);

                await _emailService.SendInvoiceEmailAsync(
                    addedEntity.Entity.User.FirstName + " " + addedEntity.Entity.User.FirstName,
                    addedEntity.Entity.User.Email,
                    invoicePdf,
                    addedEntity.Entity.OrderId.ToString(), // invoice number needs to be added here
                    addedEntity.Entity.OrderId.ToString()
                );

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        //adds the user and used coupon to UserCoupons table
        public async Task RecordUsedCoupons(Guid UserId, Guid CouponId)
        {
            _appDbContext.UserCoupons.Add(
                new UserCouponEntity()
                {
                    CouponId = CouponId,
                    CouponUseDate = DateTime.UtcNow,
                    UserId = UserId,
                }
            );
        }


        //returns coupon obj based on couponCode
        public async Task<CouponEntity?> GetCoupon(string CouponCode)
        {
            CouponEntity? coupon = await _appDbContext.Coupons.FirstOrDefaultAsync(cou => cou.CouponCode == CouponCode);

            return coupon;
        }

        //checks the validity of the coupon
        public async Task<bool> ValidateCoupon(Guid UserId, string CouponCode)
        {
            //checks if the coupon code exists
            CouponEntity? coupon = await _appDbContext.Coupons.FirstOrDefaultAsync(c => c.CouponCode == CouponCode);

            if (coupon == null) throw new NotFoundException("Required Coupon not found");

            //checks time validity of the coupon
            if (coupon.StartDate > DateTime.UtcNow && coupon.EndDate < DateTime.UtcNow) throw new NotFoundException("The coupon has already expired");

            //checks the coupons used by the user
            UserCouponEntity? userCoupon = await _appDbContext.UserCoupons.FirstOrDefaultAsync(uc => uc.UserId == UserId && uc.Coupon.CouponCode == CouponCode);

            //return true if null
            return userCoupon == null;
        }


        //fetches the product obj in a dictinary format
        public async Task<Dictionary<Guid, ProductEntity>> FetchProductsDict(List<OrderItemsDTO> orderedItems)
        {
            var productIds = orderedItems.Select(x => x.ProductId).ToList();

            var products = await _appDbContext.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            var missingIds = productIds.Except(products.Keys).ToList();

            if (missingIds.Any())
            {
                throw new NotFoundException($"Products not found for IDs: {string.Join(", ", missingIds)}");
            }

            return products;
        }



        //adds the ordered products to seperate table for 1-M 
        public async Task AddOrderItems(Guid OrderId,
                                        List<OrderItemsDTO> orderItemEntities,
                                        Dictionary<Guid, ProductEntity> products)
        {
            //looping through each ordered products
            foreach (OrderItemsDTO item in orderItemEntities)
            {
                var product = products[item.ProductId];

                //invalid quantity check
                if (product.ProductQuantity < item.quantity || item.quantity <= 0) throw new InvalidQuantityException("Invalid quantity entered for product");

                _appDbContext.OrderItems.Add(
                    new OrderItemEntity
                    {
                        OrderId = OrderId,
                        ProductId = item.ProductId,
                        LineTotal = product.ProductUnitPrice * item.quantity,
                        Quantity = item.quantity,
                        UnitPrice = product.ProductUnitPrice
                    }
                );

                await ChangeQuantityAfterOrder(product.ProductId, item.quantity);
            }
        }

        //changes the stock quantity after order
        public async Task ChangeQuantityAfterOrder(Guid ProductId, int OrderedQuantity)
        {
            ProductEntity? product = await _appDbContext.Products.FirstOrDefaultAsync(pro => pro.ProductId == ProductId);

            if (product == null) throw new NotFoundException("Product wiht the id not found");

            product.ProductQuantity -= OrderedQuantity;

            _appDbContext.Products.Update(product);
        }


        //fetches the orders
        public async Task<PagedResult<ViewOrderDTO>> ViewOrders(Guid? UserId, OrderFilterDTO filter)
        {
            var query = _appDbContext.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(ot => ot.Product)
                .Include(o => o.Coupon)
                .Include(o => o.User)
                .AsQueryable();

            // Filter by user
            if (UserId != null && UserId != Guid.Empty)
            {
                query = query.Where(o => o.UserId == UserId.Value);
            }

            // Apply filters
            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.Status == filter.Status.Value);
            }

            if (filter.PaymentMethod.HasValue)
            {
                query = query.Where(o => o.PaymentMethod == filter.PaymentMethod.Value);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(o => o.UserId == filter.UserId);
            }

            if (filter.ProductId.HasValue)
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.ProductId == filter.ProductId));
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(o => o.OrderDateTime >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(o => o.OrderDateTime <= filter.ToDate.Value);
            }

            // Order by most recent
            query = query.OrderByDescending(o => o.OrderDateTime);
            var totalCount = await query.CountAsync();

            // Apply pagination
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            var pagedOrders = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();



            List<ViewOrderDTO>? result = pagedOrders.Select(o => new ViewOrderDTO
            {
                UserId = o.UserId,
                OrderId = o.OrderId,
                FullName = o.User.FirstName + " " + o.User.LastName,
                Email = o.User.Email,
                Phone = o.User.PhoneNumber,
                OrderDateTime = o.OrderDateTime.ToString("yyyy-MM-dd"),
                AmountBeforeDiscount = o.AmountBeforeDiscount,
                AmountAfterDiscount = o.AmountAfterDiscount,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                BillingInfoId = o.BillingInfoId,
                AppliedCouponCode = o.Coupon?.CouponCode,
                CouponDiscountPercent = o.Coupon?.DiscountPercent,
                Remarks = o.Remarks,
                OrderItems = o.OrderItems.Select(oi => new ViewItemDTO
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    ProductImageUrl = oi.Product.ProductImages
                            .Select(img => img.ProductImageUrl)
                            .FirstOrDefault(),
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.LineTotal
                }).ToList()
            }).ToList();

            return new PagedResult<ViewOrderDTO>
            {
                Items = result,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }



        //changes the order status as per required
        public async Task UpdateOrderStatus(UpdateStatusDTO updateStatusDTO)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                OrderEntity? orderToUpdate = await _appDbContext.Orders.FirstOrDefaultAsync(ord => ord.OrderId == updateStatusDTO.OrderId);

                if (orderToUpdate == null) throw new NotFoundException("Order with id not found");

                if (orderToUpdate.Status == OrderStatus.Confirmed && updateStatusDTO.Status == OrderStatus.Cancelled)
                    throw new InvalidOperationException("Confirmed orders cannot be cancelled");

                orderToUpdate.Status = updateStatusDTO.Status;

                _appDbContext.Orders.Update(orderToUpdate);

                await _notificationService.CreateAndSendAsync(
                    orderToUpdate.UserId,
                    "Order Status Updated",
                    $"Your order #{orderToUpdate.OrderId} is now {updateStatusDTO.Status}"
                );

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task AddRemarks(Guid orderId, string remarks)
        {
            OrderEntity? orderToUpdate = await _appDbContext.Orders.FirstOrDefaultAsync(ord => ord.OrderId == orderId);

            if (orderToUpdate == null) throw new NotFoundException("Order doesn't exist in the system");

            orderToUpdate.Remarks = remarks;

            _appDbContext.Orders.Update(orderToUpdate);
            await _appDbContext.SaveChangesAsync();

        }


    }

}