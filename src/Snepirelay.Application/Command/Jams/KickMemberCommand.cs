using Mediator;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Jams
{
    public record KickMemberCommand(string MemberId, string TargetId) : ICommand<Result>;

    public class KickMemberCommandHandler(IJamStore store, JamMemberships memberships) : ICommandHandler<KickMemberCommand, Result>
    {
        public ValueTask<Result> Handle(KickMemberCommand command, CancellationToken cancellationToken)
        {
            var caller = JamGuards.InJam(store, memberships, command.MemberId, hostOnly: true);
            if (caller.IsFailure)
                return JamGuards.Done(Result.Failure(caller));

            var jam = caller.Value.Jam;
            if (command.TargetId == command.MemberId)
                return JamGuards.Done(Result.Failure(RelayErrors.NotAllowed));

            if (!jam.Has(command.TargetId) || store.FindMember(command.TargetId) is not { } target)
                return JamGuards.Done(Result.Failure(RelayErrors.MemberNotFound));

            memberships.Remove(jam, target, SessionReasons.YouWereKicked, SessionReasons.UserKicked);
            return JamGuards.Done(Result.Success());
        }
    }
}
