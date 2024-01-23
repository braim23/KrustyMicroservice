namespace Krusty.Services.AuthAPI.Models.Dto;

public class LoginResponseDTO
{
    public UserDTO UserDTO { get; set; }
    public string Token { get; set; }
}
