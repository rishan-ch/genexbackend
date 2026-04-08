using GeneX_Backend.Modules.Category.DTOs;
using GeneX_Backend.Modules.Category.Entities;

namespace GeneX_Backend.Modules.Category.Interface
{
    public interface ISubCategoryService
    {
        Task AddNewSubCategory(AddSubCatDTO addSubCatDTO);
        Task AddSubCatAttr(List<AddSubAttrDTO> addSubAttrDTOs, Guid subCategoryId);
        Task UpdateSubCategory(Guid SubCategoryId, UpdateSubCatDTO updateSubCatDTO);
        Task<ViewSubCatDTO> GetSubCatByID(Guid SubCatId);
        Task<List<ViewSubCatAttrDTO>> GetSubCatAttrBySubCatId(Guid SubCatId);
        Task<List<ViewSubCatDTO>> GetAllSubCategory();
        Task<List<ViewSubCatAttrDTO>> GetBySubCatId(Guid SubCatId);
        Task DeleteSubCategory(Guid SubCategoryId);
        Task<List<ViewSubCatDTO>> GetSubCatByCategoryId(Guid CategoryId);
        
    }
}