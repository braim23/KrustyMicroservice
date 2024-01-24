using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Krusty.Web.Utility;

namespace Krusty.Web.Service;

public class AuthService : IAuthService
{
    private readonly IBaseService _baseService;
    public AuthService(IBaseService baseService)
    {
        _baseService = baseService;
    }
    public async Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDTO registrationRequestDTO)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = registrationRequestDTO,
            Url = SD.AuthAPIBase + "/api/auth/AssignRole"
        }, withBearer: false);
    }

    public async Task<ResponseDto?> LoginAsync(LoginRequestDTO loginRequestDTO)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = loginRequestDTO,
            Url = SD.AuthAPIBase + "/api/auth/login"
        });
    }

    public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = registrationRequestDTO,
            Url = SD.AuthAPIBase + "/api/auth/register"
        }, withBearer: false);
    }
}
