using Mediator;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Jams
{
    public record EndJamCommand(string MemberId) : ICommand<Result>;

    public class EndJamCommandHandler(IJamStore store, JamMemberships memberships) : ICommandHandler<EndJamCommand, Result>
    {
        public ValueTask<Result> Handle(EndJamCommand command, CancellationToken cancellationToken)
        {
            var caller = JamGuards.InJam(store, memberships, command.MemberId, hostOnly: true);
            if (caller.IsFailure)
                return JamGuards.Done(Result.Failure(caller));

            memberships.End(caller.Value.Jam);
            return JamGuards.Done(Result.Success());
        }
    }
}
