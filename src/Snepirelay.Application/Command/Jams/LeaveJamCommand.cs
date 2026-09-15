using Mediator;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Jams
{
    public record LeaveJamCommand(string MemberId) : ICommand<Result>;

    public class LeaveJamCommandHandler(IJamStore store, JamMemberships memberships) : ICommandHandler<LeaveJamCommand, Result>
    {
        public ValueTask<Result> Handle(LeaveJamCommand command, CancellationToken cancellationToken)
        {
            var caller = JamGuards.InJam(store, memberships, command.MemberId);
            if (caller.IsFailure)
                return JamGuards.Done(Result.Failure(caller));

            memberships.LeaveCurrent(caller.Value.Member);
            return JamGuards.Done(Result.Success());
        }
    }
}
