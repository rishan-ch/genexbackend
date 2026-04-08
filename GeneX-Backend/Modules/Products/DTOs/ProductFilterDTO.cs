namespace GeneX_Backend.Modules.Products.DTOs
{
    public class ProductFilterDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? ProductName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? HasDiscount { get; set; }
        public bool? InStockOnly { get; set; } // quantity greater than zero
        public bool? HotDeals{ get; set; }
        
        //for dynamic attributes
        public Dictionary<string, string>? AttributeFilters { get; set; }
    }
}
