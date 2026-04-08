using System.Text.Json;

namespace GeneX_Backend.Modules.Category.DTOs
{

    public class ViewSubCatAttrDTO
    {
        public Guid AttributeId { get; set; }
        public required Guid SubCategoryId { get; set; }
        public required string AttributeName { get; set; }
        public List<string>? PossibleValuesJson { get; set; }
        public required string Type { get; set; }
        public required bool IsRequired { get; set; }

        public ViewSubCatAttrDTO(){}

    }

}