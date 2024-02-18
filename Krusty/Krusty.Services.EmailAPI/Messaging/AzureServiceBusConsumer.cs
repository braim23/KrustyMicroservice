using Azure.Messaging.ServiceBus;
using Krusty.Services.EmailAPI.Message;
using Krusty.Services.EmailAPI.Models.Dto;
using Krusty.Services.EmailAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Krusty.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string serviceBusConnectionString;
    private readonly string emailCartQueue;
    private readonly string registerUserQueue;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private readonly string orderCreated_Topic;
    private readonly string orderCreated_Email_Subscription;
    private ServiceBusProcessor _emailOrderPlacedProcessor;
    private ServiceBusProcessor _emailCartProcessor;
    private ServiceBusProcessor _registerUserProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
    {
        _configuration = configuration;
        serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

        emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
        registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
        orderCreated_Topic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        orderCreated_Email_Subscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Email_Subscription");

        var client = new ServiceBusClient(serviceBusConnectionString);
        _emailCartProcessor = client.CreateProcessor(emailCartQueue);
        _registerUserProcessor = client.CreateProcessor(registerUserQueue);
        _emailOrderPlacedProcessor = client.CreateProcessor(orderCreated_Topic, orderCreated_Email_Subscription);
        _emailService = emailService;
    }

    public async Task Start()
    {
        _emailCartProcessor.ProcessMessageAsync += onEmailCartRequestReceived;
        _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
        await _emailCartProcessor.StartProcessingAsync();

        _registerUserProcessor.ProcessMessageAsync += onUserRegisterRequestReceived;
        _registerUserProcessor.ProcessErrorAsync += ErrorHandler;
        await _registerUserProcessor.StartProcessingAsync();

        _emailOrderPlacedProcessor.ProcessMessageAsync += onOrderPlacedRequestReceived;
        _emailOrderPlacedProcessor.ProcessErrorAsync += ErrorHandler;
        await _emailOrderPlacedProcessor.StartProcessingAsync();

    }


    public async Task Stop()
    {
        await _emailCartProcessor.StopProcessingAsync();
        await _emailCartProcessor.DisposeAsync();

        await _registerUserProcessor.StopProcessingAsync();
        await _registerUserProcessor.DisposeAsync();

        await _emailOrderPlacedProcessor.StopProcessingAsync();
        await _emailOrderPlacedProcessor.DisposeAsync();
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task onEmailCartRequestReceived(ProcessMessageEventArgs args)
    {
        // we receive the message here
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body);
        try
        {
            //TODO log email
            await _emailService.EmailCartAndLog(objMessage);
            await args.CompleteMessageAsync(args.Message);
        }catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
    private async Task onOrderPlacedRequestReceived(ProcessMessageEventArgs args)
    {
        // we receive the message here
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body);
        try
        {
            //TODO log email
            await _emailService.LogOrderPlaced(objMessage);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
    private async Task onUserRegisterRequestReceived(ProcessMessageEventArgs args)
    {
        // we receive the message here
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        string email = JsonConvert.DeserializeObject<string>(body);
        try
        {
            //TODO log email
            await _emailService.RegisterUserEmailAndLog(email);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
