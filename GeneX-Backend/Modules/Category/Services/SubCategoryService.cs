using System.Text.Json;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Category.DTOs;
using GeneX_Backend.Modules.Category.Entities;
using GeneX_Backend.Modules.Category.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Category.Services
{

    public class SubCategoryService : ISubCategoryService
    {
        private readonly AppDbContext _appDbContext;

        public SubCategoryService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        //adds the entity to subcategories table
        public async Task AddNewSubCategory(AddSubCatDTO addSubCatDTO)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                //only check for duplicate if the existing category are not soft deleted
                bool exists = await _appDbContext.SubCategories.AnyAsync(
                    c => c.SubCategoryName == addSubCatDTO.SubCategoryName
                    && c.DeletedAt == null
                );
                if (exists)
                    throw new AlreadyExistsException("A category with this name already exists.");

                SubCategoryEntity EntityToSave = new SubCategoryEntity()
                {
                    SubCategoryName = addSubCatDTO.SubCategoryName,
                    CategoryId = addSubCatDTO.CategoryId
                };

                var addedSub = await _appDbContext.SubCategories.AddAsync(EntityToSave);
                await _appDbContext.SaveChangesAsync();

                //add the attributes to the table after adding the main entity
                await AddSubCatAttr(addSubCatDTO.AddSubAttrDTO, addedSub.Entity.SubCategoryId);
                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        //adds the subcategory's attributes to the database table
        public async Task AddSubCatAttr(List<AddSubAttrDTO> addSubAttrDTOs, Guid subCategoryId)
        {
            for (int i = 0; i < addSubAttrDTOs.Count; i++)
            {
                var currentAttribute = addSubAttrDTOs[i];
                SubCategoryAttributeEntity subAttr = new SubCategoryAttributeEntity()
                {
                    AttributeName = currentAttribute.AttributeName,
                    PossibleValuesJson = JsonSerializer.Serialize(currentAttribute.PossibleValuesJson),
                    Type = currentAttribute.Type,
                    IsRequired = currentAttribute.IsRequired,
                    SubCategoryId = subCategoryId
                };

                await _appDbContext.SubCategoryAttributes.AddAsync(subAttr);

            }
        }



        //get all the sub categories along with the attributes
        public async Task<List<ViewSubCatDTO>> GetAllSubCategory()
        {

            List<SubCategoryEntity> subCategories = await _appDbContext.SubCategories
                .Where(
                    sub => sub.DeletedAt == null
                ).ToListAsync();
            List<ViewSubCatDTO> subCatDTOs = new List<ViewSubCatDTO>();
            foreach (var subCategory in subCategories)
            {
                ViewSubCatDTO subCatDTO = new ViewSubCatDTO()
                {
                    SubCategoryId = subCategory.SubCategoryId,
                    CategoryId = subCategory.CategoryId,
                    SubCategoryName = subCategory.SubCategoryName,
                    SubCatAttrs = await GetBySubCatId(subCategory.SubCategoryId)
                };
                subCatDTOs.Add(subCatDTO);
            }
            return subCatDTOs;
        }




        //retrieves both sub category and attributes by SubCategoryId
        public async Task<ViewSubCatDTO> GetSubCatByID(Guid SubCatId)
        {
            SubCategoryEntity? subCat = await _appDbContext.SubCategories
                .FirstOrDefaultAsync(
                    sub => sub.SubCategoryId == SubCatId &&
                    sub.DeletedAt == null
                );
            if (subCat == null) return null;

            ViewSubCatDTO subCatDTO = new ViewSubCatDTO()
            {
                SubCategoryId = subCat.SubCategoryId,
                SubCategoryName = subCat.SubCategoryName,
                CategoryId = subCat.CategoryId,
                SubCatAttrs = await GetSubCatAttrBySubCatId(subCat.SubCategoryId)
            };

            return subCatDTO;
        }



        //fetches the subCategoryAttribute by subCategoryId
        public async Task<List<ViewSubCatAttrDTO>> GetSubCatAttrBySubCatId(Guid SubCatId)
        {
            List<SubCategoryAttributeEntity>? subCat = await _appDbContext.SubCategoryAttributes
                .Where(attr => attr.SubCategoryId == SubCatId)
                .ToListAsync();

            if (subCat == null) return null;

            List<ViewSubCatAttrDTO> allAttributes = new List<ViewSubCatAttrDTO>();

            foreach (var item in subCat)
            {
                allAttributes.Add(ConvertToDTO(item));  //converts to view DTO object
            }

            return allAttributes;

        }



        //returns subCategoryAttributes by subCategoryID
        public async Task<List<ViewSubCatAttrDTO>> GetBySubCatId(Guid SubCatId)
        {
            List<SubCategoryAttributeEntity>? attributes = await _appDbContext.SubCategoryAttributes
                .Where(sub => sub.SubCategoryId == SubCatId)
                .ToListAsync();
            List<ViewSubCatAttrDTO> subCatAttr = new List<ViewSubCatAttrDTO>();
            if (attributes.Count == 0) return new List<ViewSubCatAttrDTO>();
            foreach (var attr in attributes)
            {
                subCatAttr.Add(ConvertToDTO(attr));
            }
            return subCatAttr;
        }


        //converts the DB entity of SubCategoryAttribute to a DTO
        public ViewSubCatAttrDTO ConvertToDTO(SubCategoryAttributeEntity attr)
        {
            ViewSubCatAttrDTO subCategoryAttr = new ViewSubCatAttrDTO()
            {
                AttributeId = attr.AttributeId,
                SubCategoryId = attr.SubCategoryId,
                AttributeName = attr.AttributeName,
                PossibleValuesJson = string.IsNullOrWhiteSpace(attr.PossibleValuesJson)
                    ? null
                    : JsonSerializer.Deserialize<List<string>>(attr.PossibleValuesJson),
                Type = attr.Type,
                IsRequired = attr.IsRequired
            };

            return subCategoryAttr;
        }



        //For updating sub category
        //flow: UpdateSubCategory -->  UpdateSubCatName --> UpdateSubCatAttr or AddSubCatAttr
        public async Task UpdateSubCategory(Guid SubCategoryId, UpdateSubCatDTO updateSubCatDTO)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                //fetched subcategories from database for comparision
                SubCategoryEntity? fetchedSubCat = await _appDbContext.SubCategories
                    .FirstOrDefaultAsync(
                        sub => sub.SubCategoryId == SubCategoryId &&
                        sub.DeletedAt == null
                    );

                if (fetchedSubCat == null)
                    throw new NotFoundException("No subcategory with the given id exists in the system");

                await UpdateSubCatName(fetchedSubCat, updateSubCatDTO);

                //attributes fetched from Api
                List<UpdateSubAttrDTO>? attributes = updateSubCatDTO.Attributes;


                if (attributes != null)
                {
                    await UpdateSubCatAttr(attributes, SubCategoryId);
                }

                if (updateSubCatDTO.RemovedAttributeId != null)
                {
                    foreach (var id in updateSubCatDTO.RemovedAttributeId)
                    {
                        await DeleteSubAttr(id);
                    }
                }
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


        }



        //updating sub category's name only
        public async Task UpdateSubCatName(SubCategoryEntity fetchedSubCat, UpdateSubCatDTO updateSubCatDTO)
        {
            //checks for subcategory's name, ignoring the case differences
            if (!string.Equals(fetchedSubCat.SubCategoryName,
                updateSubCatDTO.SubCategoryName,
                StringComparison.OrdinalIgnoreCase))
            {
                fetchedSubCat.SubCategoryName = updateSubCatDTO.SubCategoryName;
                _appDbContext.SubCategories.Update(fetchedSubCat);
            }
        }




        //update the attributes of the subCategory
        public async Task UpdateSubCatAttr(List<UpdateSubAttrDTO> attributes, Guid SubCategoryId)
        {
            //going through every attributes
            foreach (UpdateSubAttrDTO subAttr in attributes)
            {
                //incase id is not null
                //if id exists = value is to be updated or unchanged
                if (subAttr.AttributeId != null)
                {
                    await UpdateExistingAttr(subAttr);

                }
                //null id = new subcategory attribute
                else if (subAttr.AttributeId == null)
                {
                    List<AddSubAttrDTO> newSubAttr = new List<AddSubAttrDTO>()
                    {
                        new AddSubAttrDTO{
                            AttributeName = subAttr.AttributeName,
                            PossibleValuesJson = subAttr.PossibleValuesJson,
                            Type = subAttr.Type,
                            IsRequired = subAttr.IsRequired
                        }
                    };
                    await AddSubCatAttr(newSubAttr, SubCategoryId);
                }
            }
        }




        //updates the existing attributes
        public async Task UpdateExistingAttr(UpdateSubAttrDTO subAttr)
        {
            //existing subcategory attribute
            SubCategoryAttributeEntity? existingAttr = await _appDbContext.SubCategoryAttributes
                .FirstOrDefaultAsync(sub => sub.AttributeId == subAttr.AttributeId);

            //if subcategory attribute is found
            if (existingAttr != null)
            {
                //checking if the attribute's value (from db) = value in Api
                //not equal = value is to be updated
                if (!String.Equals(existingAttr.AttributeName, subAttr.AttributeName) ||
                    !String.Equals(existingAttr.PossibleValuesJson, JsonSerializer.Serialize(subAttr.PossibleValuesJson)) ||
                    !String.Equals(existingAttr.Type, subAttr.Type) ||
                    existingAttr.IsRequired != subAttr.IsRequired)
                {
                    existingAttr.AttributeName = subAttr.AttributeName;
                    existingAttr.PossibleValuesJson = JsonSerializer.Serialize(subAttr.PossibleValuesJson);
                    existingAttr.Type = subAttr.Type;
                    existingAttr.IsRequired = subAttr.IsRequired;

                    _appDbContext.SubCategoryAttributes.Update(existingAttr);
                }
            }
        }

        //used during update operations
        //reomves the sub attributes
        public async Task DeleteSubAttr(Guid SubAttrId)
        {
            _appDbContext.SubCategoryAttributes.Remove(
                await _appDbContext.SubCategoryAttributes.FirstAsync(attr => attr.AttributeId == SubAttrId)
            );
        }



        //soft deletes the subcategory 
        //deletes only if no product is linked with the subcategory
        public async Task DeleteSubCategory(Guid SubCategoryId)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                SubCategoryEntity? subCategory = await _appDbContext.SubCategories
                .Include(sc => sc.Products) // This is the missing part
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == SubCategoryId && sc.DeletedAt == null);

                if (subCategory == null)
                    throw new NotFoundException("Subcategory with the id doesn't exist");

                if (subCategory.Products.Any())
                    throw new InvalidOperationException("Couldn't delete the subcategory as it is linked with some product/s");

                subCategory.DeletedAt = DateTimeOffset.UtcNow;

                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ViewSubCatDTO>> GetSubCatByCategoryId(Guid CategoryId)
        {
            List<SubCategoryEntity> subCategories = await _appDbContext.SubCategories
            .Where(
                sub => sub.DeletedAt == null &&
                sub.CategoryId == CategoryId
            ).ToListAsync();
            List<ViewSubCatDTO> subCatDTOs = new List<ViewSubCatDTO>();
            foreach (var subCategory in subCategories)
            {
                ViewSubCatDTO subCatDTO = new ViewSubCatDTO()
                {
                    SubCategoryId = subCategory.SubCategoryId,
                    CategoryId = subCategory.CategoryId,
                    SubCategoryName = subCategory.SubCategoryName,
                    SubCatAttrs = await GetBySubCatId(subCategory.SubCategoryId)
                };
                subCatDTOs.Add(subCatDTO);
            }
            return subCatDTOs;
        }
    }

}