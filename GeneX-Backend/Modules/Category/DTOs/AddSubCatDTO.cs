namespace GeneX_Backend.Modules.Category.DTOs
{

    public class AddSubCatDTO
    {
        public required string SubCategoryName { get; set; }
        public required Guid CategoryId { get; set; }

        public required List<AddSubAttrDTO> AddSubAttrDTO { get; set; }

    }
    
}