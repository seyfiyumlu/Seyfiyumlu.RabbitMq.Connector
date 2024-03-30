using Microsoft.Extensions.Hosting;
using Seyfiyumlu.RabbitMq.Connector.Models;

namespace RabbitMq.Connector.Tests;
public class TestApp : IHostedService
{
    private readonly ServerMessageQueuePublisher _serverMessageQueuePublisher;
    private readonly ServerMessageQueueConsumer _serverMessageQueueConsumer;
    private readonly RabbitMqConfiguration _rabbitMqConfiguration;

    public TestApp(ServerMessageQueuePublisher serverMessageQueuePublisher, RabbitMqConfiguration rabbitMqConfiguration,
            ServerMessageQueueConsumer serverMessageQueueConsumer)
    {
        _serverMessageQueuePublisher = serverMessageQueuePublisher;
        _serverMessageQueueConsumer = serverMessageQueueConsumer;

        _rabbitMqConfiguration = rabbitMqConfiguration;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _serverMessageQueueConsumer.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _serverMessageQueueConsumer.StopAsync(cancellationToken);
    }
}
