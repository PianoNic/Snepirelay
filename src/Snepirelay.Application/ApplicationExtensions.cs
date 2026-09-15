using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Snepirelay.Application.Behaviors;
using Snepirelay.Application.Maintenance;
using Snepirelay.Application.Services;

namespace Snepirelay.Application
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<RelayGate>();
            services.AddSingleton<ConnectionRegistry>();
            services.AddSingleton<JamNotifier>();
            services.AddSingleton<JamMemberships>();
            services.AddSingleton<RelaySocketRunner>();
            services.AddHostedService<SweepService>();
            return services;
        }
    }
}
