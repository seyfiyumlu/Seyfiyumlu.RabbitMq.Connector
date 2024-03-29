using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.Connector.Tests;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Xunit.Sdk;

namespace RabbitMq.Connector.Tests;

public class BaseTest : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IConfiguration _configuration;

    public BaseTest()
    {
        var services = new ServiceCollection();


        var projectPath = Directory.GetCurrentDirectory();

        _configuration = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appSettings.json")
            .Build();

        var startup = new TestStartup(_configuration);
        startup.ConfigureServices(services);
        //_rabbitMqService = GetRequiredService<IRabbitMqService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    protected T GetRequiredService<T>()
    {
        T _service = _serviceProvider.GetService<T>();
        return _service;
    }

}
