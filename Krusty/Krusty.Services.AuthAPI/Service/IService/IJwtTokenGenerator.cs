using Krusty.Services.AuthAPI.Models;

namespace Krusty.Services.AuthAPI.Service.IService;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
}
