using Microsoft.Extensions.Configuration;
using RabbitMq.Connector.Tests.Models;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Shouldly;

namespace RabbitMq.Connector.Tests;

public class ClientConnectorTests : BaseTest
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ServerMessageQueuePublisher _serverMessageQueuePublisher;

    public ClientConnectorTests()
    {
        _rabbitMqService = GetRequiredService<IRabbitMqService>();
        _serverMessageQueuePublisher = GetRequiredService<ServerMessageQueuePublisher>();
    }

    [Fact]
    public void SHouldConnectRabbitMq()
    {
        _rabbitMqService.TryConnect();

        _rabbitMqService.IsConnected.ShouldBeTrue();

    }

    [Fact]
    public async void ShouldPublishMessage()
    {
        _rabbitMqService.TryConnect();

        var userList = new List<UserMessage>
        {
            new UserMessage
            {
                ChatId = 870351599,
                MessageText = "905305220794",
                MsgTypeID = 0,
                CreateDate = DateTime.Now,
                SentDate = DateTime.Now
            }
        };
        await _serverMessageQueuePublisher.SendListAsync(userList);
    }

}