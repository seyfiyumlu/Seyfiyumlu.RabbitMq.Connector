using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMq.Connector.Tests.Models;
using Seyfiyumlu.RabbitMq.Connector.Implementations;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Seyfiyumlu.RabbitMq.Connector.Models;

namespace RabbitMq.Connector.Tests;

public class ServerMessageQueuePublisher : RabbitMqPublisherBase<UserMessage>
{
    public ServerMessageQueuePublisher(RabbitMqConfiguration rabbitMqConfigOptions,
            ILogger<RabbitMqPublisherBase<UserMessage>> logger, IRabbitMqService rabbitMQPersistentConnection)
        : base(logger, rabbitMQPersistentConnection, rabbitMqConfigOptions.PublisherQueueName,
            rabbitMqConfigOptions.PublisherExchangeName)
    {
    }
}