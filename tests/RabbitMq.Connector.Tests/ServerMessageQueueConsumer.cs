using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMq.Connector.Tests.Models;
using RabbitMQTester.Implementations;
using Seyfiyumlu.RabbitMq.Connector.Implementations;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Seyfiyumlu.RabbitMq.Connector.Models;

namespace RabbitMq.Connector.Tests;

public class ServerMessageQueueConsumer : RabbitMqSubscriberBase<UserMessage>
{
    public ServerMessageQueueConsumer(RabbitMqConfiguration rabbitMqConfigOptions,
            ILogger<RabbitMqSubscriberBase<UserMessage>> logger, IRabbitMqService rabbitMQPersistentConnection)
        : base(logger, rabbitMQPersistentConnection, rabbitMqConfigOptions.PublisherQueueName,
            rabbitMqConfigOptions.PublisherExchangeName)
    {
    }

    public async override Task HandleMessage(UserMessage message)
    {
        Console.WriteLine($"Message Received: {message.MessageText}");
    }
}