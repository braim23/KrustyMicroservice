using Krusty.Services.OrderAPI.Models.Dto;
using Krusty.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Krusty.Services.ShoppingCartAPI.Service;

public class ProductService : IProductService
{
    IHttpClientFactory _httpClientFactory;

    public ProductService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        var client = _httpClientFactory.CreateClient("Product");
        var respone = await client.GetAsync($"/api/product");
        var apiContent = await respone.Content.ReadAsStringAsync();
        var response2 = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
        if(response2.IsSuccess)
        {
            return JsonConvert.DeserializeObject<IEnumerable
                <ProductDto>>(Convert.ToString(response2.Result));
        }
        return new List<ProductDto>();
    }
}
