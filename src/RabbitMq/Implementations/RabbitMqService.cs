using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Events;
using Seyfiyumlu.RabbitMq.Connector.Models;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using RabbitMQ.Client;


namespace Seyfiyumlu.RabbitMq.Connector.Implementations;

public class RabbitMqService : IRabbitMqService
{
    private readonly RabbitMqConfiguration _configuration;
    private readonly ILogger<RabbitMqService> _logger;
    private bool _disposed;
    private IConnection _connection;
    private object sync_root = new object();
    private IConnectionFactory _connectionFactory;
    public RabbitMqService(RabbitMqConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        var factory = new ConnectionFactory() { DispatchConsumersAsync = true };
        factory.RequestedHeartbeat = TimeSpan.FromSeconds(configuration.Heartbeat); // Convert ushort to TimeSpan
        factory.UserName = configuration.Username;
        factory.Password = configuration.Password;
        factory.Port = configuration.Port;
        _connectionFactory = factory;
        _configuration = configuration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsConnected
    {
        get
        {
            return _connection != null && _connection.IsOpen && !_disposed;
        }
    }

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return _connection.CreateModel();
    }

    public IConnection CreateChannel()
    {
        ConnectionFactory connection = new ConnectionFactory()
        {
            UserName = _configuration.Username,
            Password = _configuration.Password,
            HostName = _configuration.Host,
            Port = _configuration.Port
        };
        connection.DispatchConsumersAsync = true;
        var channel = connection.CreateConnection();
        return channel;
    }

    public bool TryConnect()
    {
        _logger.LogInformation("RabbitMQ Client is trying to connect");

        lock (sync_root)
        {
            if (IsConnected)
            {
                _logger.LogInformation("RabbitMQ Client is already connected");
                return true;
            }
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_configuration.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                }
            );

            policy.Execute(() =>
            {
                var endpoints = GetAmqpTcpEndpoints();

                if (string.IsNullOrEmpty(_configuration.ConnectionName))
                    _connection = _connectionFactory
                        .CreateConnection(endpoints);
                else
                {
                    _connection = _connectionFactory
                        .CreateConnection(endpoints);
                }
            });

            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events", _connection.Endpoint.HostName);

                return true;
            }
            else
            {
                _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

                return false;
            }
        }
    }

    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
    {
        if (_disposed) return;

        _logger.LogWarning($"A RabbitMQ connection is shutdown. Connection blocked {e.Reason} Trying to re-connect...");

        TryConnect();
    }

    private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        if (_disposed) return;

        _logger.LogWarning($"A RabbitMQ connection throw callback exception {e.Exception} {string.Join(",", e.Detail.Values)} Trying to re-connect...");

        TryConnect();
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        if (_disposed) return;

        _logger.LogWarning($"A RabbitMQ connection is on shutdown. Reason {string.Concat("cause :", e.Cause.ToString(), " Initiator: ", e.Initiator, " ClassId: ", e.ClassId, " MethodId: ", e.MethodId, " ReplyCode: ", e.ReplyCode, " ReplyCode: ", " ReplyText: ", e.ReplyText, " : ", e.ToString())} Trying to re-connect...");

        TryConnect();
    }

    private List<AmqpTcpEndpoint> GetAmqpTcpEndpoints()
    {
        var endpoints = new List<AmqpTcpEndpoint>();


        foreach (var url in _configuration.ClusterUrls)
        {
            endpoints.Add(new AmqpTcpEndpoint(url));
        }

        return endpoints;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        try
        {
            _connection.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex.ToString());
        }
    }
}



