using Krusty.Services.RewardAPI.Message;

namespace Krusty.Services.RewardAPI.Services;

public interface IRewardService
{
    Task UpdateRewards(RewardsMessage message);
}
