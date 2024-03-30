# RabbitMq Connector Middleware

## Overview

The RabbitMq Connector Middleware is a powerful tool for integrating RabbitMQ messaging into your application. It provides a simple and efficient way to send and receive messages using RabbitMQ as the messaging broker.

## Features

- Easy setup and configuration
- Publish and consume messages with ease
- Support for message acknowledgements and rejections
- Error handling and retry mechanisms
- Extensible and customizable

## Installation

To install the RabbitMq Connector Middleware, you can use [NuGet](https://www.nuget.org/packages/RabbitMQClientLibrary/1.0.1):

## Usage

Create a config file(appSettings.json) or create a new instance of RabbitMqConfiguration class.

```json
{
  "RabbitMqConfiguration": {
    "Host": "127.0.0.1",
    "Port": 5672,
    "Username": "rabbitmq",
    "Password": "rabbitmq",
    "ConsumerQueueName": "TestAppExchange",
    "ConsumerExchangeName": "TestAppQueue",
    "PublisherQueueName": "PublishAppExchange",
    "PublisherExchangeName": "PublishAppQueue",
    "ClusterUrls": ["127.0.0.1"],
    "Heartbeat": 15,
    "RetryCount": 5,
    "ConnectionName": "TestApp"
  }
}
```

```c#
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
```

Then create a consumer class or publisher class based on `RabbitMqPublisherBase<T> ` T is model class of message exchange.

## Create a publisher class

```c#
public class ServerMessageQueuePublisher : RabbitMqPublisherBase<UserMessage>
{
    public ServerMessageQueuePublisher(RabbitMqConfiguration rabbitMqConfigOptions,
            ILogger<RabbitMqPublisherBase<UserMessage>> logger, IRabbitMqService rabbitMQPersistentConnection)
        : base(logger, rabbitMQPersistentConnection, rabbitMqConfigOptions.PublisherQueueName,
            rabbitMqConfigOptions.PublisherExchangeName)
    {
    }
}

///message exchange class
public class UserMessage
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Message { get; set; }
}
```

## Create a consumer class

```c#
public class ServerMessageQueueConsumer : RabbitMqSubscriberBase<UserMessage>
{
    public ServerMessageQueueConsumer(RabbitMqConfiguration rabbitMqConfigOptions,
            ILogger<RabbitMqSubscriberBase<UserMessage>> logger, IRabbitMqService rabbitMQPersistentConnection)
        : base(logger, rabbitMQPersistentConnection, rabbitMqConfigOptions.ConsumerQueueName,
            rabbitMqConfigOptions.ConsumerExchangeName)
    {
    }

    public async override Task HandleMessage(UserMessage message)
    {
        Console.WriteLine($"Message Received: {message.Message}");
    }
}
```

## Startup.cs

Add methods below into the Startup.cs.

```c#
    private void AddRabbitMq(IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConfig = configuration.GetSection("RabbitMqConfiguration").Get<RabbitMqConfiguration>();
        services.Configure<RabbitMqConfiguration>(configuration.GetSection("RabbitMqConfiguration"));
        services.AddSingleton<RabbitMqConfiguration>(rabbitMqConfig);
        services.AddSingleton<IRabbitMqService, RabbitMqService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();
            var config = sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
            return new RabbitMqService(config, logger);
        });
    }

    private void AddRabbitMqPublisher(IServiceCollection services)
    {
        services.AddScoped<ServerMessageQueuePublisher>();
    }
    private void AddRabbitMqConsumer(IServiceCollection services)
    {
        services.AddHostedService<ServerMessageQueueConsumer>();
    }

```

## ConfigureServices :

```c#
    AddRabbitMq(context.Services, configuration);
    AddRabbitMqPublisher(context.Services);
    AddRabbitMqConsumer(context.Services);
```

## Then Use Middleware into the app builder

```c#
app.UseRabbitMqConnector();
```

## Usage :

- Consumer :
  In ServerMessageQueueConsumer class has coming with an abstract method named : HandleMessage. you can use this method to process messages from queue.

```c#
    public async override Task HandleMessage(UserMessage message)
    {
        Console.WriteLine($"Message Received: {message.Message}");
    }
```

- Publisher :
  Use DI for use the publisher class.

```c#
private readonly ServerMessageQueuePublisher _serverMessageQueuePublisher;

public Dashboard(ServerMessageQueuePublisher serverMessageQueuePublisher)
{
    _serverMessageQueuePublisher = serverMessageQueuePublisher;
}

///you can use this part any of your code.
var message = new UserMessage();
message = new UserMessage
{
    UserId = "1", // "1" is the admin user id, it should be fetched from the current user's id
    UserName = "admin",
    Message = "Dashboard page is visited"
};

await _serverMessageQueuePublisher.SendAsync(message);

```

Thank you.
