using RabbitMQ.Client;

namespace Seyfiyumlu.RabbitMq.Connector.Interfaces;

public interface IRabbitMqService : IDisposable
{
    bool IsConnected { get; }

    bool TryConnect();

    IModel CreateModel();
    IConnection CreateChannel();
}