using Microsoft.AspNetCore.Builder;

namespace Seyfiyumlu.RabbitMq.Connector;

public static class RabbitMqExtensions
{
    public static IApplicationBuilder UseRabbitMqConnector(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RabbitMqConnectorMiddleware>();
    }
}
