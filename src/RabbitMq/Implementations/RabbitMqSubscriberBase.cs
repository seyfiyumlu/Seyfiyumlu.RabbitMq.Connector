
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;


namespace RabbitMQTester.Implementations;

public abstract class RabbitMqSubscriberBase<T> : BackgroundService
{
    private readonly IRabbitMqService _rabbitMQPersistentConnection;
    private readonly string _queue;
    private readonly Guid _channelId;
    private readonly ILogger<RabbitMqSubscriberBase<T>> _logger;
    private IModel _channel;

    public RabbitMqSubscriberBase(ILogger<RabbitMqSubscriberBase<T>> logger, IRabbitMqService rabbitMQPersistentConnection, string queue)
    {
        _logger = logger;
        _rabbitMQPersistentConnection = rabbitMQPersistentConnection;
        _queue = queue;
        _channelId = Guid.NewGuid();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        CreateChannel(_queue);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += ConsumerReceived;
        _channel.BasicConsume(_queue, false, consumer);

        return Task.CompletedTask;
    }

    private async Task ConsumerReceived(object sender, BasicDeliverEventArgs @event)
    {
        try
        {
            _logger.LogInformation("Message processing {body}", Encoding.UTF8.GetString(@event.Body.ToArray()));
            if (@event.Body.Length > 5)
            {
                var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                await HandleMessage(message);
            }
            _channel.BasicAck(@event.DeliveryTag, false);
        }
        catch (Exception ex) when (ex.Message.Contains("Cannot access a disposed object"))
        {
            _logger.LogWarning("ConsumerReceived Error:" + Encoding.UTF8.GetString(@event.Body.ToArray()) + "Exception:" + ErrorMessage(ex));
            _channel.BasicNack(@event.DeliveryTag, false, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unhandled ConsumerReceived Error:" + Encoding.UTF8.GetString(@event.Body.ToArray()) + "Exception:" + ErrorMessage(ex));
            _channel.BasicAck(@event.DeliveryTag, false);
        }
    }

    public abstract Task HandleMessage(T message);

    private void CreateChannel(string queue)
    {
        var policy = Policy.Handle<Exception>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "RabbitMQ Client could not create channel after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                }
            );

        policy.Execute(() =>
        {
            if (!_rabbitMQPersistentConnection.IsConnected)
            {
                _rabbitMQPersistentConnection.TryConnect();
            }

            _channel = _rabbitMQPersistentConnection.CreateModel();
            _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false,
                arguments: null);
            _channel.BasicQos(0, 5, false);
            _logger.LogInformation($"RabbitMq  {_channelId} Channel Opened!");
        });
    }

    public override void Dispose()
    {
        if (_channel != null)
        {
            _channel.Close();
            _channel.Dispose();
        }
        base.Dispose();
    }

    private string ErrorMessage(Exception ex)
    {
        string errMessage = string.Concat("ExMessage:", ex.Message, "InnerException:", ex.InnerException?.Message, "Stack Trace:", ex.StackTrace);
        if (errMessage.Length > 4000)
            errMessage = errMessage.Substring(0, 3998);
        return errMessage;
    }


}