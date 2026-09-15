using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Snepirelay.Application.Command.Maintenance;

namespace Snepirelay.Application.Maintenance
{
    public class SweepService(IMediator mediator, IOptions<RelayOptions> options, TimeProvider time) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(options.Value.SweepInterval, time);
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await mediator.Send(new SweepCommand(), stoppingToken);
        }
    }
}
