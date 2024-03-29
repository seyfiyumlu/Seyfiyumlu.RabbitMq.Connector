using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Serilog;
using Seyfiyumlu.RabbitMq.Connector.Implementations;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Seyfiyumlu.RabbitMq.Connector.Models;

namespace RabbitMq.Connector.Tests;
public static class ServiceBuilder
{

    // public static void ConfigureRabbitmqService(this IServiceCollection services, IConfiguration configuration)
    // {
    //     services.Configure<RabbitMqConfiguration>(a => configuration.GetSection(nameof(RabbitMqConfiguration)).Bind(a));
    //     services.AddSingleton<IRabbitMqService>(sp =>
    //     {
    //         //
    //         var rabbitMqConfig = configuration.GetSection(nameof(RabbitMqConfiguration)).Get<RabbitMqConfiguration>();
    //         var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();

    //         var factory = new ConnectionFactory() { DispatchConsumersAsync = true };
    //         factory.RequestedHeartbeat = TimeSpan.FromSeconds(rabbitMqConfig.Heartbeat); // Convert ushort to TimeSpan
    //         factory.UserName = rabbitMqConfig.Username;
    //         factory.Password = rabbitMqConfig.Password;
    //         factory.Port = rabbitMqConfig.Port;

    //         return new RabbitMqService(rabbitMqConfig, factory, logger);
    //     });
    // }

    public static void AddLogger(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var rabbitMqConfig = new RabbitMqConfiguration
        {
            Host = "127.0.0.1",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            Heartbeat = 30,
            ClusterUrls = ["127.0.0.1"],
            ConnectionName = "TestApp",
            ConsumerExchangeName = "TestAppExchange",
            ConsumerQueueName = "TestAppQueue",
            PublisherExchangeName = "PublishAppExchange",
            PublisherQueueName = "PublishAppQueue",
            RetryCount = 3
        };
    }


    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConfig = new RabbitMqConfiguration
        {
            Host = "127.0.0.1",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            Heartbeat = 30,
            ClusterUrls = ["127.0.0.1"],
            ConnectionName = "TestApp",
            ConsumerExchangeName = "TestAppExchange",
            ConsumerQueueName = "TestAppQueue",
            PublisherExchangeName = "PublishAppExchange",
            PublisherQueueName = "PublishAppQueue",
            RetryCount = 3
        };

        var rabbitMqConfig = configuration.GetSection(nameof(RabbitMqConfiguration)).Get<RabbitMqConfiguration>();
        services.AddSingleton<RabbitMqConfiguration>(rabbitMqConfig);
        services.AddLogging();
        services.AddLogger(configuration);
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        // services.AddHostedService<SubscriberService>();
        services.AddHostedService<TestApp>(sp =>
        {
            return new TestApp(sp.GetRequiredService<ServerMessageQueuePublisher>(), rabbitMqConfig);
        });

        services.AddScoped<ServerMessageQueuePublisher>();
    }

}