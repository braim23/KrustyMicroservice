using Azure.Messaging.ServiceBus;
using Krusty.Services.RewardAPI.Message;
using Krusty.Services.RewardAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Krusty.Services.RewardAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string serviceBusConnectionString;
    private readonly string orderCreatedTopic;
    private readonly string orderCreatedRewardSubscription;
    private readonly IConfiguration _configuration;
    private readonly RewardService _rewardService;

    private ServiceBusProcessor _rewardProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
    {
        _rewardService = rewardService;
        _configuration = configuration;
        serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

        orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");

        var client = new ServiceBusClient(serviceBusConnectionString);
        _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);
    }

    public async Task Start()
    {
        _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
        _rewardProcessor.ProcessErrorAsync += ErrorHandler;
        await _rewardProcessor.StartProcessingAsync();

    }


    public async Task Stop()
    {
        await _rewardProcessor.StopProcessingAsync();
        await _rewardProcessor.DisposeAsync();

    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs args)
    {
        // we receive the message here
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body);
        try
        {
            //TODO log email
            await _rewardService.UpdateRewards(objMessage);
            await args.CompleteMessageAsync(args.Message);
        }catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
    
}
