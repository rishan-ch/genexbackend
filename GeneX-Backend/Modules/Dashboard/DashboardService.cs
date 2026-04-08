using System.Dynamic;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Orders.DTOs;
using GeneX_Backend.Modules.Products.DTOs;
using GeneX_Backend.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Dashbaord
{
    public class DashboardService
    {
        private readonly AppDbContext _appDbContext;

        public DashboardService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Dictionary<string, decimal>> GetDashboardDetails()
        {
            var data = new Dictionary<string, decimal>();

            // Total Income (sum of order amounts)
            decimal totalIncome = await _appDbContext.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.AmountAfterDiscount) ?? 0;

            // Total Orders (all orders count)
            decimal totalOrders = await _appDbContext.Orders.CountAsync();

            // Completed Orders count
            decimal completedOrders = await _appDbContext.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .CountAsync();

            // Total Customers count
            decimal totalCustomers = await _appDbContext.Users
                .Where(u => _appDbContext.UserRoles.Any(ur => ur.UserId == u.Id &&
                              _appDbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Customer")))
                .CountAsync();


            // Add main dashboard entries
            data.Add("totalIncome", totalIncome);
            data.Add("totalOrders", totalOrders);
            data.Add("completedOrders", completedOrders);
            data.Add("totalCustomers", totalCustomers);

            // Sales by category
            var salesByCategory = await _appDbContext.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed)  // Only confirmed orders
                .GroupBy(oi => oi.Product.SubCategoryEntity.CategoryEntity)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalSales = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .ToListAsync();

            foreach (var categorySale in salesByCategory)
            {
                string key = categorySale.Category.CategoryName;
                data[key] = categorySale.TotalSales;
            }

            return data;
        }

        public async Task<List<OrderDashboardDTO>> Recent5Orders()
        {
            var orders = await _appDbContext.Orders
                .OrderByDescending(o => o.OrderDateTime)
                .Include(o => o.User)
                .Take(5)
                .Select(ord => new OrderDashboardDTO
                {
                    FullName = ord.User.FirstName + " " + ord.User.LastName,
                    AmountAfterDiscount = ord.AmountAfterDiscount,
                    OrderDateTime = ord.OrderDateTime.ToString("yyyy-MM-dd"),
                    OrderId = ord.OrderId,
                    Status = ord.Status

                })
                .ToListAsync();

            return orders;
        }

        public async Task<List<TopProductDTO>> GetTop5MostSoldProducts()
        {
            var topProducts = await _appDbContext.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed && !oi.Product.IsDeleted)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    ProductName = g.First().Product.ProductName,
                    ImageUrl = g.First().Product.ProductImages
                            .Select(img => img.ProductImageUrl)
                            .FirstOrDefault(),
                    Category = g.First().Product.SubCategoryEntity.CategoryEntity.CategoryName,
                    Stock = g.First().Product.ProductQuantity,
                    Price = g.First().Product.ProductUnitPrice
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            // Map to DTO
            return topProducts.Select(p => new TopProductDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductImageUrl = p.ImageUrl,
                TotalQuantitySold = p.TotalQuantitySold,
                Category = p.Category,
                Stock = p.Stock,
                Price = p.Price

            }).ToList();
        }

        public async Task<Dictionary<string, decimal>> SalesByMonth(string fromDate, string toDate)
        {
            if (!DateTime.TryParseExact(fromDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime fromDateParsed))
                throw new ArgumentException("Invalid fromDate format. Expected yyyy-MM-dd");

            if (!DateTime.TryParseExact(toDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime toDateParsed))
                throw new ArgumentException("Invalid toDate format. Expected yyyy-MM-dd");

            // Specify UTC kind
            fromDateParsed = DateTime.SpecifyKind(fromDateParsed, DateTimeKind.Utc);
            toDateParsed = DateTime.SpecifyKind(toDateParsed, DateTimeKind.Utc);

            // Ensure toDateParsed includes the full day
            toDateParsed = toDateParsed.Date.AddDays(1).AddTicks(-1);

            var revenueByMonth = await _appDbContext.Orders
                .Where(o => o.Status == OrderStatus.Completed
                            && o.OrderDateTime >= fromDateParsed
                            && o.OrderDateTime <= toDateParsed)
                .GroupBy(o => new { o.OrderDateTime.Year, o.OrderDateTime.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(o => o.AmountAfterDiscount)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var dict = revenueByMonth.ToDictionary(
                x => $"{x.Year}-{x.Month:D2}",
                x => x.TotalRevenue);

            return dict;
        }

    }
}