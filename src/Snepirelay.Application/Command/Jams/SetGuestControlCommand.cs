using Mediator;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Domain.Enums;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Jams
{
    public record SetGuestControlCommand(string MemberId, GuestControl GuestControl) : ICommand<Result>;

    public class SetGuestControlCommandHandler(IJamStore store, JamMemberships memberships, JamNotifier notifier, TimeProvider time)
        : ICommandHandler<SetGuestControlCommand, Result>
    {
        public ValueTask<Result> Handle(SetGuestControlCommand command, CancellationToken cancellationToken)
        {
            var caller = JamGuards.InJam(store, memberships, command.MemberId, hostOnly: true);
            if (caller.IsFailure)
                return JamGuards.Done(Result.Failure(caller));

            var jam = caller.Value.Jam;
            jam.GuestControl = command.GuestControl;
            jam.Touch(time.GetUtcNow());
            notifier.Broadcast(jam, SessionReasons.SettingsUpdated, []);

            return JamGuards.Done(Result.Success());
        }
    }
}
