using System.Text.Json;
using System.Text.Json.Serialization;
using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.API.Controllers;
using Snepirelay.Application;
using Snepirelay.Application.Behaviors;
using Snepirelay.Infrastructure.Extensions;

namespace Snepirelay.API.Extensions
{
    public static class SnepirelayExtensions
    {
        public const string LivePath = "/health/live";

        public static IServiceCollection AddSnepirelay(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RelayOptions>().Bind(configuration.GetSection(RelayOptions.Section));

            services.AddControllers()
                .AddApplicationPart(typeof(AppController).Assembly)
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

            services.AddMediator((MediatorOptions options) =>
            {
                options.ServiceLifetime = ServiceLifetime.Singleton;
                options.PipelineBehaviors = [typeof(SerializedBehavior<,>)];
            });

            services.AddApplication();
            services.AddInfrastructure();
            services.AddHealthChecks();

            return services;
        }

        public static WebApplication MapSnepirelay(this WebApplication app)
        {
            var options = app.Services.GetRequiredService<IOptions<RelayOptions>>().Value;

            app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = options.KeepAliveInterval });
            app.MapControllers();
            app.MapHealthChecks(LivePath);

            return app;
        }
    }
}
