using GeneX_Backend.Infrastructure.CloudinaryService;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Discount.Entities;
using GeneX_Backend.Modules.Products.DTOs;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Products.Interfaces;
using GeneX_Backend.Shared.Enums;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeneX_Backend.Modules.Discount.Interface;

namespace GeneX_Backend.Modules.Products.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IDiscountService _discountService;

        public ProductService(AppDbContext context, CloudinaryService cloudinaryService, IDiscountService discountService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _discountService = discountService;
        }

        private List<ProductAttributeDTO> DeserializeProductAttributes(string json)
        {
            try
            {
                var attributes = JsonSerializer.Deserialize<List<ProductAttributeDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (attributes == null || !attributes.Any())
                    throw new ArgumentException("No attributes provided. Please provide at least one attribute.");

                return attributes;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid JSON format for Attributes.", ex);
            }
        }

        private void ValidateAddProductDTO(AddProductDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ProductName))
                throw new ArgumentException("Product name cannot be empty.");

            if (dto.ProductQuantity < 0)
                throw new ArgumentException("Product quantity cannot be negative.");

            if (dto.ProductUnitPrice <= 0)
                throw new ArgumentException("Product price must be greater than zero.");

            bool subCatExists = _context.SubCategories.Any(sc => sc.SubCategoryId == dto.SubCategoryId);
            if (!subCatExists)
                throw new NotFoundException("SubCategory not found.");

            bool productExists = _context.Products.Any(p =>
                p.ProductName == dto.ProductName &&
                p.SubCategoryId == dto.SubCategoryId);
            if (productExists)
                throw new AlreadyExistsException("Product already exists in this subcategory.");
        }

        private List<ProductAttributeEntity> BuildAttributeEntities(List<ProductAttributeDTO> attrDtos, Guid productId, Guid subCategoryId)
        {
            var attributeEntities = new List<ProductAttributeEntity>();

            foreach (var attrDto in attrDtos)
            {
                var subAttr = _context.SubCategoryAttributes.FirstOrDefault(x =>
                    x.AttributeId == attrDto.SubCategoryAttributeId &&
                    x.SubCategoryId == subCategoryId);

                if (subAttr == null)
                    throw new ArgumentException($"Invalid attribute ID: {attrDto.SubCategoryAttributeId}");

                if (!string.IsNullOrWhiteSpace(subAttr.PossibleValuesJson))
                {
                    var allowedValues = JsonSerializer.Deserialize<List<string>>(subAttr.PossibleValuesJson);

                    if (!string.IsNullOrEmpty(attrDto.ProductAttributeValue))
                    {
                        var selectedValues = attrDto.ProductAttributeValue.Split(',').Select(v => v.Trim()).ToList();
                        var invalidValues = selectedValues.Except(allowedValues).ToList();

                        if (invalidValues.Any())
                        {
                            throw new ArgumentException($"Invalid value(s) '{string.Join(", ", invalidValues)}' for attribute '{subAttr.AttributeName}'");
                        }
                    }
                }


                attributeEntities.Add(new ProductAttributeEntity
                {
                    ProductAttributeId = Guid.NewGuid(),
                    ProductId = productId,
                    SubCategoryAttributeId = attrDto.SubCategoryAttributeId,
                    SubCategoryAttributeName = attrDto.SubCategoryAttributeName,
                    ProductAttributeType = subAttr.Type,
                    ProductAttributeValue = attrDto.ProductAttributeValue
                });
            }

            return attributeEntities;
        }

        public async Task<int> CalculateTotalSales(Guid ProductId)
        {
            return await _context.OrderItems
            .Where(item => item.ProductId == ProductId && item.Order.Status == OrderStatus.Completed)
            .SumAsync(item => item.Quantity);
        }

        private async Task<ViewProductDTO> MaptoViewProduct(ProductEntity p)
        {
            var discount = p.DiscountEntity != null ? p.DiscountEntity : null;

            decimal? discountPercentage = discount?.DiscountPercentage;
            decimal? discountedPrice = discountPercentage.HasValue
                ? Math.Round(p.ProductUnitPrice * (1 - discountPercentage.Value / 100), 2)
                : null;

            return new ViewProductDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName ?? throw new InvalidDataException($"Invalid product data : {p.ProductId}"),
                ProductDescription = p.ProductDescription,
                ProductImageUrl = await _context.ProductImages.Where(pi => pi.ProductId == p.ProductId).Select(p => p.ProductImageUrl).ToListAsync(),
                ProductUnitPrice = p.ProductUnitPrice,
                DiscountId = p.DiscountId,
                DiscountedPrice = discountedPrice,
                DiscountPercentage = discountPercentage,
                ProductQuantity = p.ProductQuantity,
                CategoryId = p.SubCategoryEntity.CategoryId,
                CategoryName = p.SubCategoryEntity.CategoryEntity.CategoryName,
                SubCategoryId = p.SubCategoryId,
                SubCategoryName = p.SubCategoryEntity?.SubCategoryName,
                Sales = await CalculateTotalSales(p.ProductId),
                ProductStatus = p.ProductQuantity == 0 ? "Out-of-Stock" : "In-Stock",
                Hotdeals = p.HotDeals,
                AvgRating = p.ReviewEntities.Any()
                    ? p.ReviewEntities.Average(r => r.StarCount)
                    : 0,
                ReviewCount = p.ReviewEntities.Count(r => r.ProductId == p.ProductId)
            };
        }

        private async Task<ViewProductDTO> MaptoProductAnAttributes(ProductEntity p)
        {
            var discount = p.DiscountEntity != null ? p.DiscountEntity : null;

            decimal? discountPercentage = discount?.DiscountPercentage;
            decimal? discountedPrice = discountPercentage.HasValue
                ? Math.Round(p.ProductUnitPrice * (1 - discountPercentage.Value / 100), 2)
                : null;

            return new ViewProductDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName ?? throw new InvalidDataException($"Invalid product data : {p.ProductId}"),
                ProductDescription = p.ProductDescription,
                ProductImageUrl = await _context.ProductImages.Where(pi => pi.ProductId == p.ProductId).Select(p => p.ProductImageUrl).ToListAsync(),
                ProductUnitPrice = p.ProductUnitPrice,
                DiscountId = p.DiscountId,
                DiscountedPrice = discountedPrice,
                DiscountPercentage = discountPercentage,
                ProductQuantity = p.ProductQuantity,
                CategoryId = p.SubCategoryEntity.CategoryId,
                CategoryName = p.SubCategoryEntity.CategoryEntity.CategoryName,
                SubCategoryId = p.SubCategoryId,
                SubCategoryName = p.SubCategoryEntity?.SubCategoryName,
                Sales = await CalculateTotalSales(p.ProductId),
                ProductStatus = p.ProductQuantity == 0 ? "Out-of-Stock" : "In-Stock",
                Hotdeals = p.HotDeals,
                AvgRating = p.ReviewEntities.Any()
                    ? p.ReviewEntities.Average(r => r.StarCount)
                    : 0,
                ReviewCount = p.ReviewEntities.Count(r => r.ProductId == p.ProductId),
                Attributes = await GetAllAttributes(p.ProductId)
            };
        }

        public async Task<List<ViewProductAttributeDTO>> GetAllAttributes(Guid ProductId)
        {
            return await _context.ProductAttributes
            .Where(
                p => p.ProductId == ProductId
            )
            .Select(
                p => new ViewProductAttributeDTO
                {
                    ProductAttributeId = p.ProductAttributeId,
                    ProductAttributeName = p.SubCategoryAttributeName,
                    ProductAttributeValue = p.ProductAttributeValue != null ? p.ProductAttributeValue : null,
                    SubCategoryAttributeId = p.SubCategoryAttributeId
                }
            )
            .ToListAsync();
        }


        public async Task<bool> AddProduct(AddProductDTO dto)
        {
            ValidateAddProductDTO(dto);




            var attributes = DeserializeProductAttributes(dto.Attributes);

            // Check for valid discount first
            DiscountEntity? discount = null;
            if (dto.DiscountId.HasValue)
            {
                discount = await _context.Discounts
                    .FirstOrDefaultAsync(d => d.DiscountId == dto.DiscountId);

                if (discount == null)
                    throw new NotFoundException($"Discount with ID {dto.DiscountId} not found or is deleted.");

                // Optionally check for expiration
                if (discount.ValidTill <= DateTime.UtcNow)
                    throw new InvalidOperationException("Cannot assign expired discount.");
            }

            var product = new ProductEntity
            {
                ProductId = Guid.NewGuid(),
                SubCategoryId = dto.SubCategoryId,
                ProductName = dto.ProductName,
                ProductDescription = dto.ProductDescription,
                ProductQuantity = dto.ProductQuantity,
                ProductUnitPrice = dto.ProductUnitPrice,
                HotDeals = dto.HotDeals,
                AddedDate = DateTime.UtcNow,
                DiscountId = discount?.DiscountId  // Assign the discount ID if discount exists
            };

            Console.WriteLine($"Product DiscountId before save: {product.DiscountId}");

            product.ProductAttributes = BuildAttributeEntities(attributes, product.ProductId, dto.SubCategoryId);


            if (dto.ProductImage != null)
            {
                foreach (IFormFile item in dto.ProductImage)
                {
                    string? imageUrl = dto.ProductImage != null
                        ? await _cloudinaryService.UploadImageAsync(item)
                        : null;

                    await _context.ProductImages.AddAsync(
                        new ProductImagesEntity
                        {
                            ProductId = product.ProductId,
                            ProductImageUrl = imageUrl
                        }
                    );
                }
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Verify the saved product
            var savedProduct = await _context.Products
                .Include(p => p.DiscountEntity)
                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

            Console.WriteLine($"Saved Product DiscountId: {savedProduct?.DiscountId}");

            return true;
        }

        public async Task<PagedResult<ViewProductDTO>> FetchAllProductsAsync(ProductFilterDTO filter)
        {
            await _discountService.CleanUpExistingDiscountAsync();

            var query = _context.Products
                .Include(p => p.SubCategoryEntity).ThenInclude(sub => sub.CategoryEntity)
                .Include(p => p.ProductAttributes).ThenInclude(pa => pa.SubCategoryAttribute)
                .Include(p => p.DiscountEntity)
                .Where(p => !p.IsDeleted);


            if (!string.IsNullOrWhiteSpace(filter.CategoryName))
            {
                query = query.Where(q => q.SubCategoryEntity.CategoryEntity.CategoryName.Contains(filter.CategoryName));
            }

            if (!string.IsNullOrWhiteSpace(filter.SubCategoryName))
            {
                query = query.Where(q => q.SubCategoryEntity.SubCategoryName.Contains(filter.SubCategoryName));
            }

            if (!string.IsNullOrWhiteSpace(filter.ProductName))
            {
                query = query.Where(p => p.ProductName.Contains(filter.ProductName));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.ProductUnitPrice >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.ProductUnitPrice <= filter.MaxPrice.Value);
            }

            if (filter.HasDiscount.HasValue)
            {
                if (filter.HasDiscount.Value)
                {
                    query = query.Where(p => p.DiscountId != null);
                }
                else
                {
                    query = query.Where(p => p.DiscountId == null || p.DiscountEntity == null);
                }
            }

            if (filter.InStockOnly.HasValue && filter.InStockOnly.Value)
            {
                query = query.Where(p => p.ProductQuantity > 0);
            }
            if (filter.InStockOnly.HasValue && !filter.InStockOnly.Value)
            {
                query = query.Where(p => p.ProductQuantity == 0);
            }

            if (filter.HotDeals.HasValue)
            {
                query = query.Where(p => p.HotDeals);
            }

            if (filter.AttributeFilters != null)
            {
                foreach (var attr in filter.AttributeFilters)
                {
                    query = query.Where(p =>
                        p.ProductAttributes.Any(pa =>
                            pa.SubCategoryAttributeName == attr.Key &&
                            pa.ProductAttributeValue == attr.Value));
                }
            }

            int totalCount = await query.CountAsync();

           
            decimal? lowestPrice = null;
            decimal? highestPrice = null;

            if (totalCount > 0)
            {
                lowestPrice = await query.MinAsync(p => (decimal?)p.ProductUnitPrice);
                highestPrice = await query.MaxAsync(p => (decimal?)p.ProductUnitPrice);
            }

  
            filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);
            int skip = (filter.PageNumber - 1) * filter.PageSize;

            // 4. Apply pagination
            var products = await query
                .OrderByDescending(p => p.ProductName) // your preferred ordering
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            // 5. Map to DTOs
            var resultItems = new List<ViewProductDTO>();
            foreach (var product in products)
            {
                resultItems.Add(await MaptoViewProduct(product));
            }

            // 6. Calculate discounted price range for current page
            decimal? discountedLowestPrice = null;
            decimal? discountedHighestPrice = null;

            var discountedPrices = resultItems
                .Where(p => p.DiscountedPrice.HasValue)
                .Select(p => p.DiscountedPrice.Value)
                .ToList();

            if (discountedPrices.Any())
            {
                discountedLowestPrice = discountedPrices.Min();
                discountedHighestPrice = discountedPrices.Max();
            }

            // 7. Compare original and discounted price ranges and pick final min/max
            decimal? lowestValueRange = null;
            decimal? highestValueRange = null;

            if (lowestPrice.HasValue && discountedLowestPrice.HasValue)
                lowestValueRange = Math.Min(lowestPrice.Value, discountedLowestPrice.Value);
            else
                lowestValueRange = lowestPrice ?? discountedLowestPrice;

            if (highestPrice.HasValue && discountedHighestPrice.HasValue)
                highestValueRange = Math.Max(highestPrice.Value, discountedHighestPrice.Value);
            else
                highestValueRange = highestPrice ?? discountedHighestPrice;

            // 8. Return paged result with final value ranges
            return new PagedResult<ViewProductDTO>
            {
                Items = resultItems,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                lowestValueRange = lowestValueRange,
                highestValueRange = highestValueRange
            };
        }



        public async Task<ViewProductDTO> FetchProductByIdAsync(Guid productId)
        {

            await _discountService.CleanUpExistingDiscountAsync();

            if (productId == Guid.Empty)
                throw new ArgumentException("Invalid product ID.");

            ProductEntity? product = await _context.Products
                .Include(p => p.SubCategoryEntity).ThenInclude(s => s.CategoryEntity)
                .Include(p => p.ReviewEntities)
                .Include(p => p.ProductAttributes).ThenInclude(attr => attr.SubCategoryAttribute)
                .Include(p => p.DiscountEntity)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                throw new NotFoundException("Product not found.");

            if (product.IsDeleted)
                throw new NotFoundException("Product has been deleted");

            return await MaptoProductAnAttributes(product);
        }


        //get Related Products 
        public async Task<List<ViewProductDTO>> FetchProductBySubCateIdAsync(Guid subCategoryId)
        {
            if (subCategoryId == Guid.Empty)
                throw new ArgumentException("Invalid SubcategoryId.");

            var products = await _context.Products
                .Include(p => p.SubCategoryEntity).ThenInclude(sub => sub.CategoryEntity)
                .Include(p => p.ProductAttributes).ThenInclude(attr => attr.SubCategoryAttribute)
                .Include(p => p.DiscountEntity)
                .Where(p => p.SubCategoryId == subCategoryId && !p.IsDeleted)
                .ToListAsync();

            if (products == null || products.Count == 0)
                throw new NotFoundException("No products found for the given subcategory.");

            var result = new List<ViewProductDTO>();
            foreach (var product in products)
            {
                result.Add(await MaptoViewProduct(product));
            }

            return result;
        }


        public async Task<bool> UpdateProductAsync(Guid productId, UpdateProductDTO dto)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Invalid product  ID.");

            var product = await _context.Products
                .IgnoreQueryFilters()
                .Include(p => p.ProductAttributes)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                throw new NotFoundException("Product not found.");

            if (product.IsDeleted)
                throw new InvalidOperationException("Cannot update a deleted product.");

            product.ProductName = dto.ProductName;
            product.ProductDescription = dto.ProductDescription;
            product.ProductQuantity = dto.ProductQuantity;
            product.ProductUnitPrice = dto.ProductUnitPrice;
            product.HotDeals = dto.Hotdeals;

            if (dto.ProductImage != null)
            {

                List<ProductImagesEntity> images = await _context.ProductImages.Where(p => p.ProductId == product.ProductId).ToListAsync();
                foreach (ProductImagesEntity item in images)
                {
                    _context.ProductImages.Remove(item);
                }

                foreach (IFormFile item in dto.ProductImage)
                {
                    string? imageUrl = dto.ProductImage != null
                        ? await _cloudinaryService.UploadImageAsync(item)
                        : null;

                    await _context.ProductImages.AddAsync(
                        new ProductImagesEntity
                        {
                            ProductId = product.ProductId,
                            ProductImageUrl = imageUrl
                        }
                    );
                }
            }

            var existingAttributes = await _context.ProductAttributes
                .IgnoreQueryFilters()
                .Where(p => p.ProductId == product.ProductId)
                .ToListAsync();

            _context.ProductAttributes.RemoveRange(existingAttributes);

            var newAttributes = DeserializeProductAttributes(dto.Attributes);
            var newAttributeEntities = BuildAttributeEntities(newAttributes, product.ProductId, product.SubCategoryId);
            await _context.ProductAttributes.AddRangeAsync(newAttributeEntities);

            product.DiscountId = null;

            if (dto.DiscountId != null)
            {
                var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountId == dto.DiscountId);
                if (discount == null)
                    throw new NotFoundException("Discount not found.");

                product.DiscountId = discount.DiscountId;
                product.DiscountEntity = discount;
            }

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteProduct(Guid id)
        {
            var product = await _context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return false;

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ViewProductDTO>> GetNewProducts(int count)
        {
            List<ProductEntity>? recentProducts = await _context.Products
                .Include(p => p.SubCategoryEntity)
                .Include(p => p.DiscountEntity)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.AddedDate)
                .Take(count)
                .ToListAsync();

            List<ViewProductDTO>? result = new List<ViewProductDTO>();

            foreach (var product in recentProducts.Where(p => p != null))
            {
                result.Add(await MaptoViewProduct(product));
            }


            return result;
        }

        public async Task<List<ViewProductDTO>> GetHotDealsProducts(int count)
        {
            List<ProductEntity>? hotdeals = await _context.Products
                .Include(p => p.SubCategoryEntity)
                    .ThenInclude(sub => sub.CategoryEntity)
                .Include(p => p.DiscountEntity)
                .Where(p => !p.IsDeleted && p.HotDeals)
                .Take(count)
                .ToListAsync();


            List<ViewProductDTO>? result = new List<ViewProductDTO>();

            foreach (var product in hotdeals.Where(p => p != null))
            {
                result.Add(await MaptoViewProduct(product));
            }


            return result;
        }


        public async Task<List<ViewProductDTO>> GetTopRatedProducts(int count)
        {
            List<ProductEntity>? products = await _context.Products
                .Include(p => p.SubCategoryEntity)
                    .ThenInclude(sub => sub.CategoryEntity)
                .Include(p => p.DiscountEntity)
                .Include(p => p.ReviewEntities)
                .Where(p => !p.IsDeleted)
                .ToListAsync();


            List<ProductEntity>? topRated = products
    .Select(p => new
    {
        Product = p,
        AverageRating = (p.ReviewEntities != null && p.ReviewEntities.Any())
            ? p.ReviewEntities.Average(r => r.StarCount)
            : 0
    })
    .OrderByDescending(x => x.AverageRating)
    .Take(count)
    .Select(x => x.Product)
    .ToList();


            var result = new List<ViewProductDTO>();
            foreach (var product in topRated.Where(p => p != null))
            {
                result.Add(await MaptoViewProduct(product));
            }


            return result;
        }

    }
}