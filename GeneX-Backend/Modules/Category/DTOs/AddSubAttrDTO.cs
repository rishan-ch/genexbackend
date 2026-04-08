namespace GeneX_Backend.Modules.Category.DTOs
{

    public class AddSubAttrDTO
    {
        public required string AttributeName { get; set; }
        public List<string>? PossibleValuesJson { get; set; }
        public required string Type { get; set; }
        public required bool IsRequired { get; set; }
    }

}