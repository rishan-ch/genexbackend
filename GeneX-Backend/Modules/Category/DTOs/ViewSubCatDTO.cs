using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace GeneX_Backend.Modules.Category.DTOs
{

    public class ViewSubCatDTO
    {
        public Guid SubCategoryId { get; set; }
        public required string SubCategoryName { get; set; }
        public required Guid CategoryId { get; set; }

        public required List<ViewSubCatAttrDTO> SubCatAttrs { get; set; }

    }
    
}