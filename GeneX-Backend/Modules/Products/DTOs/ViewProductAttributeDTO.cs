namespace GeneX_Backend.Modules.Products.DTOs
{
    public class ViewProductAttributeDTO
    {

        public Guid SubCategoryAttributeId { get; set; }
        public Guid ProductAttributeId { get; set; }
        public string? ProductAttributeName { get; set; }
        public string? ProductAttributeValue { get; set; }
   
    }
}
