using Mediator;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Jams
{
    public record CreateJamCommand(string MemberId) : ICommand<Result>;

    public class CreateJamCommandHandler(IJamStore store, JamMemberships memberships, JamNotifier notifier)
        : ICommandHandler<CreateJamCommand, Result>
    {
        public ValueTask<Result> Handle(CreateJamCommand command, CancellationToken cancellationToken)
        {
            if (store.FindMember(command.MemberId) is not { } member)
                return JamGuards.Done(Result.Failure(RelayErrors.HelloRequired));

            var jam = memberships.Create(member);
            notifier.SendSession(jam, member.Id, SessionReasons.YouJoined, [member.Id]);

            return JamGuards.Done(Result.Success());
        }
    }
}
