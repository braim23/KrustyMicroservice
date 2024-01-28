using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Krusty.Web.Utility;

namespace Krusty.Web.Service;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IBaseService _baseService;
    public ShoppingCartService(IBaseService baseService)
    {
        _baseService = baseService;
    }

    public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = cartDto,
            Url = SD.ShoppingCartAPIBase + "/api/cart"
        });
    }

    public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ShoppingCartAPIBase + "/api/cart/GetCart/" + userId
        });
    }
    

    public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = cartDetailsId,
            Url = SD.ShoppingCartAPIBase + "/api/cart/RemoveCoupon" + cartDetailsId
        });
    }
    

    public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = cartDto,
            Url = SD.ShoppingCartAPIBase + "/api/cart/CartUpsert"
        });
    }
}
