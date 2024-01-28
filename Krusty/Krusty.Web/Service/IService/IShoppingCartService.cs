using Krusty.Web.Models;

namespace Krusty.Web.Service.IService;

public interface IShoppingCartService
{
    Task<ResponseDto?> GetCartByUserIdAsync(string userId);
    Task<ResponseDto?> UpsertCartAsync(CartDto cartDto);
    Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId);
    Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto);
}
