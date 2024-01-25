using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Krusty.Web.Utility;

namespace Krusty.Web.Service;

public class ProductService : IProductService
{
    private readonly IBaseService _baseService;
    public ProductService(IBaseService baseService)
    {
        _baseService = baseService;
    }

    public async Task<ResponseDto?> CreateProductAsync(ProductDto ProductDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = ProductDto,
            Url = SD.ProductAPIBase + "/api/product/"
        });
    }

    public async Task<ResponseDto?> DeleteProductAsync(int id)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.DELETE,
            Url = SD.ProductAPIBase + "/api/product/" + id
        });
    }

    public async Task<ResponseDto?> GetAllProductsAsync()
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product"
        });
    }

    public async Task<ResponseDto?> GetProductAsync(string ProductCode)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product/GetByCode" + ProductCode
        });
    }

    public async Task<ResponseDto?> GetProductByIdAsync(int id)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product/" + id
        });
    }

    public async Task<ResponseDto?> UpdateProductAsync(ProductDto ProductDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.PUT,
            Data = ProductDto,
            Url = SD.ProductAPIBase + "/api/product/"
        });
    }
}
