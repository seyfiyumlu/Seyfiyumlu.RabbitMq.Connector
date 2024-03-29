using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seyfiyumlu.RabbitMq.Connector;
using Seyfiyumlu.RabbitMq.Connector.Implementations;
using Seyfiyumlu.RabbitMq.Connector.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seyfiyumlu.RabbitMq.Connector.Models;
using Microsoft.AspNetCore.Hosting; // Add this using directive

namespace RabbitMq.Connector.Tests;

public class TestStartup
{

    private readonly IConfiguration _configuration;
    public TestStartup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //services.ConfigureRabbitmqService(_configuration);
        services.Configure<RabbitMqConfiguration>(_configuration.GetSection("RabbitMq"));
        services.AddSingleton<IRabbitMqService, RabbitMqService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();
            var config = sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
            return new RabbitMqService(config, logger);
        });

        services.RegisterServices(_configuration);
        services.UseRabbitMqConnectorMiddleware();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<RabbitMqConnectorMiddleware>();

        // Diğer middleware ve routing ayarları...
    }

}