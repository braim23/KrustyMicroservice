using Krusty.Services.RewardAPI.Data;
using Krusty.Services.RewardAPI.Message;
using Krusty.Services.RewardAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace Krusty.Services.RewardAPI.Services;

public class RewardService : IRewardService
{
    private DbContextOptions<AppDbContext> _dbOptions;

    public RewardService(DbContextOptions<AppDbContext> options)
    {
        _dbOptions = options;
    }

    
    public async Task UpdateRewards(RewardsMessage rewardsMessage)
    {
        try
        {
            Rewards rewards = new()
            {
                OrderId = rewardsMessage.OrderId,
                RewardsActivity = rewardsMessage.RewardsActivity,
                UserId = rewardsMessage.UserId,
                RewardsDate = DateTime.Now
            };
            await using var _db = new AppDbContext(_dbOptions);
            await _db.Rewards.AddAsync(rewards);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            
        }
    }

}
