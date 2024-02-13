using Krusty.Web.Models;

namespace Krusty.Web.Service.IService;

public interface IOrderService
{
    Task<ResponseDto?> CreateOrder(CartDto cartDto);
    //Task<ResponseDto?> Meow();
}
