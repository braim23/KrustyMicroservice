using Krusty.Services.ShoppingCartAPI.Models.Dto;
using Krusty.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Krusty.Services.ShoppingCartAPI.Service;

public class CouponService : ICouponService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CouponService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CouponDto> GetCouponAsync(string couponCode)
    {
        var client = _httpClientFactory.CreateClient("Coupon");
        var respone = await client.GetAsync($"/api/coupon/GetByCode/{couponCode}");
        var apiContent = await respone.Content.ReadAsStringAsync();
        var response2 = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
        if (response2.IsSuccess)
        {
            return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response2.Result));
        }
        return new CouponDto();
    }

}
