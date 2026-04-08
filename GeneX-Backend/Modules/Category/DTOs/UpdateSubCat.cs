namespace GeneX_Backend.Modules.Category.DTOs
{
    public class UpdateSubCatDTO
    {
        public string? SubCategoryName { get; set; }
        public Guid? CategoryId { get; set; }
        public List<UpdateSubAttrDTO>? Attributes { get; set; }
        public List<Guid>? RemovedAttributeId { get; set; }
    }

    public class UpdateSubAttrDTO
    {
        public Guid? AttributeId { get; set; }
        public required string AttributeName { get; set; }
        public List<string>? PossibleValuesJson { get; set; }
        public required string Type { get; set; }
        public required bool IsRequired { get; set; }
    }
}