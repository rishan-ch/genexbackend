using GeneX_Backend.Modules.Category.DTOs;

namespace GeneX_Backend.Modules.Category.Interface
{
    public interface ICategoryService
    {
        Task AddNewCategory(string CategoryName);
        Task UpdateCategory(Guid CategoryId, string CategoryName);
        ViewCategoryDTO GetCategoryById(Guid CategoryId);
        List<ViewCategoryDTO> GetAllCategory();

        Task DeleteCategory(Guid CategoryId);
    }
}