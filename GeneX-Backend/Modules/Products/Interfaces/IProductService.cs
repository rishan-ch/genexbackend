using GeneX_Backend.Modules.Products.DTOs;
using System.Threading.Tasks;

namespace GeneX_Backend.Modules.Products.Interfaces
{
    public interface IProductService
    {
        Task<bool> AddProduct(AddProductDTO dto);
        Task<PagedResult<ViewProductDTO>> FetchAllProductsAsync(ProductFilterDTO filters);
        Task<ViewProductDTO> FetchProductByIdAsync(Guid productId);

        Task<List<ViewProductDTO>> FetchProductBySubCateIdAsync(Guid SubCatId);
        Task<bool> DeleteProduct(Guid id);
        Task<bool> UpdateProductAsync(Guid productId, UpdateProductDTO dto);


        Task<List<ViewProductDTO>> GetNewProducts(int count);
        Task<List<ViewProductDTO>> GetHotDealsProducts(int count);
        Task<List<ViewProductDTO>> GetTopRatedProducts(int count);


    }
}
