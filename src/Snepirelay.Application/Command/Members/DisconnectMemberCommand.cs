using Mediator;
using Snepirelay.Application.Interfaces;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Members
{
    public record DisconnectMemberCommand(string MemberId, IRelayConnection Connection) : ICommand<Result>;

    public class DisconnectMemberCommandHandler(
        IJamStore store,
        ConnectionRegistry connections,
        JamMemberships memberships,
        JamNotifier notifier,
        TimeProvider time) : ICommandHandler<DisconnectMemberCommand, Result>
    {
        public ValueTask<Result> Handle(DisconnectMemberCommand command, CancellationToken cancellationToken)
        {
            if (!connections.Detach(command.MemberId, command.Connection) || store.FindMember(command.MemberId) is not { } member)
                return ValueTask.FromResult(Result.Success());

            var now = time.GetUtcNow();
            member.Connected = false;
            member.DisconnectedAt = now;
            member.LastSeenAt = now;

            if (memberships.JamOf(member) is { } jam)
            {
                jam.Touch(now);
                notifier.Broadcast(jam, SessionReasons.UserUpdated, [member.Id], except: member.Id);
            }

            return ValueTask.FromResult(Result.Success());
        }
    }
}
