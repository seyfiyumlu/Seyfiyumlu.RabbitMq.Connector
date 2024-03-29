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

Add this class into the DI container.

```c#
services.AddSingleton<RabbitMqConfiguration>(rabbitMqConfig);
services.AddScoped<ServerMessageQueuePublisher>();
```

Then add `IRabbitMqService` with logger and configuration

```c#
services.AddSingleton<IRabbitMqService, RabbitMqService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();
    var config = sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
    return new RabbitMqService(config, logger);
});
```

```c#
app.UseMiddleware<RabbitMqConnectorMiddleware>();
```

Thank you.
