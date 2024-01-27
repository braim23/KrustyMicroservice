using Krusty.Services.ShoppingCartAPI.Models.Dto;

namespace Krusty.Services.ShoppingCartAPI.Service.IService;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();
}

