using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Users.DTOs
{
    public class ViewUserFilterDto
    {
        public required int pageNumber { get; set; }
        public required int pageSize { get; set; }
    }
}
