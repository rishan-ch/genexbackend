using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GeneX_Backend.Modules.Category.DTOs
{
    public class ViewCategoryDTO
    {
        public Guid CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public required int SubCategoryCount{ get; set; }
        public required int ProductCount{ get; set; }
        public List<string>? ProductNames { get; set; }
    }
}