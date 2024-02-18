using Krusty.Services.EmailAPI.Message;
using Krusty.Services.EmailAPI.Models.Dto;

namespace Krusty.Services.EmailAPI.Services;

public interface IEmailService
{
    Task EmailCartAndLog(CartDto cartDto);
    Task RegisterUserEmailAndLog(string email);
    Task LogOrderPlaced(RewardsMessage rewardsDto);
}
