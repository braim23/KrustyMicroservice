﻿using Krusty.Services.EmailAPI.Messaging;

namespace Krusty.Services.EmailAPI.Extension;

public static class ApplicationBuilderExtensions
{
    private static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }
    public static IApplicationBuilder UserAzureServiceBusConsumer(this IApplicationBuilder app)
    {
        ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();

        var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

        hostApplicationLife.ApplicationStarted.Register(OnStart);
        hostApplicationLife.ApplicationStopping.Register(OnStop);

        return app;

    }

    private static void OnStop()
    {
        ServiceBusConsumer.Stop();
    }

    private static void OnStart()
    {
        ServiceBusConsumer.Start();
    }
}
