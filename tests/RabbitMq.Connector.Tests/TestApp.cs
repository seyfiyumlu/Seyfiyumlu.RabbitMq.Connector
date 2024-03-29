using Microsoft.Extensions.Hosting;
using Seyfiyumlu.RabbitMq.Connector.Models;

namespace RabbitMq.Connector.Tests;
public class TestApp : IHostedService
{
    private readonly ServerMessageQueuePublisher _serverMessageQueuePublisher;
    private readonly RabbitMqConfiguration _rabbitMqConfiguration;

    public TestApp(ServerMessageQueuePublisher serverMessageQueuePublisher, RabbitMqConfiguration rabbitMqConfiguration)
    {
        _serverMessageQueuePublisher = serverMessageQueuePublisher;
        _rabbitMqConfiguration = rabbitMqConfiguration;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
