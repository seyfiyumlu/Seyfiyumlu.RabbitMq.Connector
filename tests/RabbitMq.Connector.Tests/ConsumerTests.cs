using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Shouldly;

namespace RabbitMq.Connector.Tests;

public class ConsumerTests : BaseTest
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ServerMessageQueueConsumer _serverMessageQueueConsumer;

    public ConsumerTests()
    {
        _rabbitMqService = GetRequiredService<IRabbitMqService>();
        _serverMessageQueueConsumer = GetRequiredService<ServerMessageQueueConsumer>();
    }

    [Fact]
    public void ShouldConnectRabbitMq()
    {
        _rabbitMqService.TryConnect();

        _rabbitMqService.IsConnected.ShouldBeTrue();

    }

    [Fact]
    public async void ShouldConsumeMessage()
    {
        _rabbitMqService.TryConnect();
        await _serverMessageQueueConsumer.StartAsync(default);

    }

}