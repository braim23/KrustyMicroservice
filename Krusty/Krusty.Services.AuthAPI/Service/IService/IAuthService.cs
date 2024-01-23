using Krusty.Services.AuthAPI.Models.Dto;

namespace Krusty.Services.AuthAPI.Service.IService;

public interface IAuthService
{
    Task<string> Register(RegistrationRequestDTO registrationRequestDTO);
    Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
    Task<bool> AssignRole(string email, string roleName);
}
