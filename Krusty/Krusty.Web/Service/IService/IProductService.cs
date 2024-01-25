using Krusty.Web.Models;

namespace Krusty.Web.Service.IService;

public interface IProductService
{
    //Task<ResponseDto?> GetProduct(string ProductCode);
    Task<ResponseDto?> GetAllProductsAsync();
    Task<ResponseDto?> GetProductByIdAsync(int id);
    Task<ResponseDto?> CreateProductAsync(ProductDto ProductDto);
    Task<ResponseDto?> UpdateProductAsync(ProductDto ProductDto);
    Task<ResponseDto?> DeleteProductAsync(int id);
}
