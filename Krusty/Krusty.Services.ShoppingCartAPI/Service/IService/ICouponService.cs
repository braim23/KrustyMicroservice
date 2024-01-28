using Krusty.Services.ShoppingCartAPI.Models.Dto;

namespace Krusty.Services.ShoppingCartAPI.Service.IService;

public interface ICouponService
{
    Task<CouponDto> GetCouponAsync(string couponCode);
}

