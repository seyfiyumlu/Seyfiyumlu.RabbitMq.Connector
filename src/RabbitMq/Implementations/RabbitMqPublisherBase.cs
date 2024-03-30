using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client.Exceptions;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;

namespace Seyfiyumlu.RabbitMq.Connector.Implementations;

public abstract class RabbitMqPublisherBase<T>
{
    private readonly IRabbitMqService _rabbitMQPersistentConnection;
    private readonly ILogger<RabbitMqPublisherBase<T>> _logger;
    private readonly Guid _channelId;
    private readonly int _retryCount;
    protected string _queue;
    protected string _exchange;

    public RabbitMqPublisherBase(ILogger<RabbitMqPublisherBase<T>> logger,
        IRabbitMqService rabbitMQPersistentConnection, string queue, string exchange, int retryCount = 5)
    {
        _rabbitMQPersistentConnection = rabbitMQPersistentConnection;
        _logger = logger;
        _queue = queue;
        _exchange = exchange;
        _retryCount = retryCount;
        _channelId = Guid.NewGuid();
    }

    public async Task SendAsync(T message)
    {
        string ms = message.ToJson();
        var body = Encoding.UTF8.GetBytes(ms);

        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) =>
                {
                    _logger.LogWarning(ex,
                        "Could not publish event: {message} after {Timeout}s ({ExceptionMessage})", message,
                        $"{time.TotalSeconds:n1}", ex.Message);
                });

        if (!_rabbitMQPersistentConnection.IsConnected)
            _rabbitMQPersistentConnection.TryConnect();
        policy.Execute(() =>
        {
            using (var channel = _rabbitMQPersistentConnection.CreateModel())
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                channel.BasicPublish(
                    exchange: _exchange,
                    routingKey: _queue,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            }
        });
    }

    public async Task SendListAsync(List<T> message)
    {
        string ms = message.ToJson();
        var body = Encoding.UTF8.GetBytes(ms);

        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) =>
                {
                    _logger.LogWarning(ex,
                        "Could not publish event: {message} after {Timeout}s ({ExceptionMessage})", message,
                        $"{time.TotalSeconds:n1}", ex.Message);
                });

        if (!_rabbitMQPersistentConnection.IsConnected)
            _rabbitMQPersistentConnection.TryConnect();
        policy.Execute(() =>
        {
            using (var channel = _rabbitMQPersistentConnection.CreateModel())
            {
                _rabbitMQPersistentConnection.CreateQueue(channel, _exchange, _queue);
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                channel.BasicPublish(
                    exchange: _exchange,
                    routingKey: _queue,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            }
        });
    }


}