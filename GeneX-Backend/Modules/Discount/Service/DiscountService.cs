using CloudinaryDotNet.Actions;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Discount.DTOs;
using GeneX_Backend.Modules.Discount.Entities;
using GeneX_Backend.Modules.Discount.Interface;
using GeneX_Backend.Modules.Notification.Service;
using GeneX_Backend.Modules.Products.DTOs;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;



public class DiscountService : IDiscountService
{
    private readonly AppDbContext _appDbContext;
    private readonly NotificationService _notificationService;

    public DiscountService(AppDbContext appDbContext, NotificationService notification)
    {
        _appDbContext = appDbContext;
        _notificationService = notification;
    }

    // For adding new discount
    public async Task<bool> AddDiscountAsync(AddDiscountDTO discountDto)
    {
        // Validate DiscountPercentage
        if (discountDto.DiscountPercentage <= 0)
            throw new ArgumentException("Discount percentage must be greater than zero.", nameof(discountDto.DiscountPercentage));

        // Validate ValidFrom Date
        if (discountDto.ValidFrom == default)
            throw new ArgumentException("ValidFrom date must be provided and valid.", nameof(discountDto.ValidFrom));

        if (discountDto.ValidTill <= discountDto.ValidFrom)
            throw new ArgumentException("Valid Till Date cannot be before the Valid From Date", nameof(discountDto.ValidTill));


        // Check for duplicate discount
        bool exists = await _appDbContext.Discounts.AnyAsync(d =>
            d.DiscountPercentage == discountDto.DiscountPercentage
            );

        if (exists)
            throw new AlreadyExistsException("A discount with percentage already exists.");

        // Create new discount entity
        DiscountEntity newDiscount = new DiscountEntity
        {
            DiscountPercentage = discountDto.DiscountPercentage,
            ValidFrom = discountDto.ValidFrom,
            ValidTill = discountDto.ValidTill,
        };

        _appDbContext.Discounts.Add(newDiscount);
        var changes = await _appDbContext.SaveChangesAsync();
        await _notificationService.SendToAllUsersAsync("Discount offers are ongoing!", "New discount offers have been introduced to the system. Do check it out");
        return changes > 0;
    }


    // getting the list of discounts 
    public async Task<PagedResult<ViewDiscountDTO>> GetAllDiscount(ViewDiscountFilterDto filter)
    {

        await CleanUpExistingDiscountAsync();

        var query = _appDbContext.Discounts
            .Select(d => new ViewDiscountDTO
            {
                DiscountId = d.DiscountId,
                DiscountPercentage = d.DiscountPercentage,
                ValidFrom = d.ValidFrom.ToString("MMMM dd, yyyy hh:mm tt"),
                ValidTill = d.ValidTill.ToString("MMMM dd, yyyy hh:mm tt"),
            });

        var totalCount = await query.CountAsync();

        var discounts = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();


        if (!discounts.Any())
            throw new NotFoundException("No discounts found.");

        return new PagedResult<ViewDiscountDTO>
        {
            Items = discounts,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    // fetching discount by id
    public async Task<ViewDiscountDTO> GetDiscountById(Guid discountId)
    {
        await CleanUpExistingDiscountAsync();

        var discount = await _appDbContext.Discounts
            .FirstOrDefaultAsync(d => d.DiscountId == discountId);

        if (discount == null)
            throw new NotFoundException("No such discount found");

        return new ViewDiscountDTO
        {
            DiscountId = discount.DiscountId,
            DiscountPercentage = discount.DiscountPercentage,
            ValidFrom = discount.ValidFrom.ToString("MMMM dd, yyyy hh:mm tt"),
            ValidTill = discount.ValidTill.ToString("MMMM dd, yyyy hh:mm tt"),
        };
    }


    public async Task<bool> DeleteDiscountAsync(Guid discountId)
    {
        var discount = await _appDbContext.Discounts
            .FirstOrDefaultAsync(d => d.DiscountId == discountId);
        if (discount == null)
            throw new NotFoundException("Discount not found");
        _appDbContext.Discounts.Remove(discount);
        var result = await _appDbContext.SaveChangesAsync();
        return result > 0;
    }


    public async Task CleanUpExistingDiscountAsync()
    {
        var expiredDiscounts = await _appDbContext.Discounts
            .Where(d => d.ValidTill < DateTime.UtcNow)
            .ToListAsync();

        if (expiredDiscounts.Any())
        {
            var exipiredDiscountIds = expiredDiscounts.Select(d => d.DiscountId).ToList();


            //Find Products with that disocunt ID
            var productsWithExpiredDiscounts = await _appDbContext.Products
                .Where(p => p.DiscountId.HasValue && exipiredDiscountIds.Contains(p.DiscountId.Value))
                .ToListAsync();

            //Set the DiscountiD id of those products to null:
            foreach (var product in productsWithExpiredDiscounts)
            {
                product.DiscountId = null;
            }

            //Removeal of expired discounts
            _appDbContext.Discounts.RemoveRange(expiredDiscounts);
            await _appDbContext.SaveChangesAsync();
        }
    }
}


