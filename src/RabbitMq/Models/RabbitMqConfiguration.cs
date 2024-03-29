namespace Seyfiyumlu.RabbitMq.Connector.Models;
public class RabbitMqConfiguration
{
    public static readonly string Configuration = "RabbitMqConfiguration";

    public string Host { get; set; }

    public int Port { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string ConsumerQueueName { get; set; }

    public string ConsumerExchangeName { get; set; }

    public string PublisherQueueName { get; set; }

    public string PublisherExchangeName { get; set; }
    public string[] ClusterUrls { get; set; }
    public ushort Heartbeat { get; set; }
    public int RetryCount { get; set; }
    public string ConnectionName { get; set; }
}
