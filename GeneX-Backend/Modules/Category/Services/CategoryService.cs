using System.Transactions;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Category.DTOs;
using GeneX_Backend.Modules.Category.Entities;
using GeneX_Backend.Modules.Category.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GeneX_Backend.Modules.Category.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _appDbContext;

        public CategoryService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        //adds the cateegory 
        public async Task AddNewCategory(string name)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Category name cannot be empty.", nameof(name));

                //only check for duplicate if the existing category are not soft deleted
                bool exists = await _appDbContext.Categories.AnyAsync(c => c.CategoryName == name && c.DeletedAt == null);
                if (exists)
                    throw new AlreadyExistsException("A category with this name already exists.");

                CategoryEntity newCategory = new CategoryEntity()
                {
                    CategoryName = name
                };

                _appDbContext.Categories.Add(newCategory);
                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        //fiters the category which has not been deleted yet
        public List<ViewCategoryDTO> GetAllCategory()
        {
            var categoryList = _appDbContext.Categories
                .Include(c => c.SubCategories)
                    .ThenInclude(sub => sub.Products)
                .Where(c => c.DeletedAt == null)
                .Select(c => new ViewCategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    SubCategoryCount = c.SubCategories.Count(sub => sub.DeletedAt == null),
                    ProductCount = c.SubCategories
                        .Where(sub => sub.DeletedAt == null)
                        .Sum(sub => sub.Products.Count(p => p.DeletedAt == null)),
                    ProductNames = c.SubCategories
                        .Where(sub => sub.DeletedAt == null)
                        .SelectMany(sub => sub.Products)
                        .Where(p => p.DeletedAt == null)
                        .Select(p => p.ProductName)
                        .ToList()
                })
                .ToList();

            if (!categoryList.Any())
                throw new NotFoundException("No categories found");

            return categoryList;
        }


        //fetches the category even if the category is soft deleted
        public ViewCategoryDTO GetCategoryById(Guid CategoryId)
        {
            var category = _appDbContext.Categories
                .Include(c => c.SubCategories)
                    .ThenInclude(sub => sub.Products)
                .FirstOrDefault(c => c.CategoryId == CategoryId);

            if (category == null)
                throw new NotFoundException("Category not found");

            return new ViewCategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                SubCategoryCount = category.SubCategories.Count,
                ProductCount = category.SubCategories.Sum(sub => sub.Products.Count)
            };
        }


        public async Task UpdateCategory(Guid CategoryId, string newCategoryName)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                if (newCategoryName.IsNullOrEmpty()) throw new ArgumentException("Category name is empty");

                //fetches the category from database
                var fetchedCategory = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == CategoryId && c.DeletedAt == null);

                if (fetchedCategory == null) throw new NotFoundException("Category with id not found");

                //checks forname duplicacy
                bool exists = await _appDbContext.Categories.AnyAsync(c => c.CategoryName == newCategoryName);
                if (exists)
                    throw new AlreadyExistsException("A category with this name already exists.");

                fetchedCategory.CategoryName = newCategoryName;

                _appDbContext.Categories.Update(fetchedCategory);
                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }


        //adds the time stamp for DeletedAt attribute
        //soft delete
        public async Task DeleteCategory(Guid CategoryId)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                CategoryEntity? category = await _appDbContext.Categories.FirstOrDefaultAsync(cat => cat.CategoryId == CategoryId && cat.DeletedAt == null);

                if (category == null) throw new KeyNotFoundException();

                Guid CategoryIdToRemove = category.CategoryId;

                category.DeletedAt = DateTimeOffset.UtcNow;

                _appDbContext.Categories.Update(category);

                await DeleteSubCategory(CategoryIdToRemove);

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //soft delete   
        public async Task DeleteSubCategory(Guid CategoryId)
        {
            List<SubCategoryEntity> SubCategoriesToDelete = await _appDbContext.SubCategories.Where(
                subCat => subCat.CategoryId == CategoryId
            ).ToListAsync();

            if (SubCategoriesToDelete.Count() == 0) return;

            foreach (SubCategoryEntity subCategory in SubCategoriesToDelete)
            {
                subCategory.DeletedAt = DateTimeOffset.UtcNow;
                _appDbContext.SubCategories.Update(subCategory);
            }

            await _appDbContext.SaveChangesAsync();
        }
    }
}