using Microsoft.Extensions.DependencyInjection;
using Snepirelay.Infrastructure.Interfaces;
using Snepirelay.Infrastructure.Services;

namespace Snepirelay.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IJamStore, InMemoryJamStore>();
            return services;
        }
    }
}
