using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Seyfiyumlu.RabbitMq.Connector.Implementations;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Seyfiyumlu.RabbitMq.Connector.Models;

namespace Seyfiyumlu.RabbitMq.Connector;

public class RabbitMqConnectorMiddleware : IMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RabbitMqConnectorMiddleware> _logger;
    private readonly IRabbitMqService _rabbitMqService;

    public RabbitMqConnectorMiddleware(RequestDelegate next, ILogger<RabbitMqConnectorMiddleware> logger,
                    IConfiguration configuration, IRabbitMqService rabbitMQService)
    {
        _rabbitMqService = rabbitMQService;
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            if (!_rabbitMqService.IsConnected)
            {
                _rabbitMqService.TryConnect();
            }
            if (_rabbitMqService.IsConnected)
            {
                await _next(context);
            }
            else
            {
                await HandleExceptionAsync(context, new Exception("RabbitMQ Service is not connected"));
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Generate an error response based on the exception
        var response = new { error = exception.Message, stacktrace = exception.StackTrace };

        var payload = JsonSerializer.Serialize(response);
        _logger.LogError($"Unexpected Error: {payload}");
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsync(payload);
    }


}

public static class RabbitMqConnectorMiddlewareExtensions
{
    public static IServiceCollection UseRabbitMqConnectorMiddleware(this IServiceCollection services)
    {
        services.AddTransient<RabbitMqConnectorMiddleware>();
        return services;
    }
}

