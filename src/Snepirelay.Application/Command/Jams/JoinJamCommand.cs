using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Jams
{
    public record JoinJamCommand(string MemberId, string JoinToken) : ICommand<Result>;

    public class JoinJamCommandHandler(IJamStore store, JamMemberships memberships, JamNotifier notifier, IOptions<RelayOptions> options)
        : ICommandHandler<JoinJamCommand, Result>
    {
        public ValueTask<Result> Handle(JoinJamCommand command, CancellationToken cancellationToken)
        {
            if (store.FindMember(command.MemberId) is not { } member)
                return JamGuards.Done(Result.Failure(RelayErrors.HelloRequired));

            if (store.FindJamByToken(command.JoinToken) is not { } jam)
                return JamGuards.Done(Result.Failure(RelayErrors.SessionNotFound));

            if (!jam.Has(member.Id))
            {
                var max = options.Value.MaxMemberCount;
                if (jam.Members.Count >= max)
                    return JamGuards.Done(Result.Failure(RelayErrors.SessionFull, new Dictionary<string, string> { ["maxMemberCount"] = $"{max}" }));

                memberships.Add(jam, member);
                notifier.Broadcast(jam, SessionReasons.UserJoined, [member.Id], except: member.Id);
            }

            notifier.SendSession(jam, member.Id, SessionReasons.YouJoined, [member.Id]);
            notifier.SendPlayback(jam, member.Id);

            return JamGuards.Done(Result.Success());
        }
    }
}
