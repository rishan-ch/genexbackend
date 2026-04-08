namespace GeneX_Backend.Modules.Products.DTOs
{
    public class PagedResultWithPriceRange<T> : PagedResult<T>
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
