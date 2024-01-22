using Krusty.Web.Models;

namespace Krusty.Web.Service.IService;

public interface IBaseService
{
   Task<ResponseDto?> SendAsync(RequestDto requestDto);
}
