using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Maintenance
{
    public record SweepCommand : ICommand<Result>;

    public class SweepCommandHandler(IJamStore store, JamMemberships memberships, IOptions<RelayOptions> options, TimeProvider time)
        : ICommandHandler<SweepCommand, Result>
    {
        public ValueTask<Result> Handle(SweepCommand command, CancellationToken cancellationToken)
        {
            var now = time.GetUtcNow();

            foreach (var member in store.Members().Where(m => !m.Connected))
            {
                if (member.DisconnectedAt is { } since && now - since >= options.Value.ReconnectGrace)
                    memberships.LeaveCurrent(member);

                if (member.JamId is null && now - member.LastSeenAt >= options.Value.IdentityTtl)
                    store.RemoveMember(member);
            }

            return ValueTask.FromResult(Result.Success());
        }
    }
}
