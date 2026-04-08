using Org.BouncyCastle.Asn1.Mozilla;

namespace GeneX_Backend.Modules.Products.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public decimal? lowestValueRange { get; set; }
        public decimal? highestValueRange { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } 
        public int PageSize { get; set; }
    }
}
