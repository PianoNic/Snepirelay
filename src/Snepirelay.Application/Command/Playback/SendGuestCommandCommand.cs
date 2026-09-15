using Mediator;
using Snepirelay.Application.Command.Jams;
using Snepirelay.Application.Dtos.Relay;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Playback
{
    public record SendGuestCommandCommand(string MemberId, RelayCommand Command) : ICommand<Result>;

    public class SendGuestCommandCommandHandler(IJamStore store, JamMemberships memberships, JamNotifier notifier)
        : ICommandHandler<SendGuestCommandCommand, Result>
    {
        public ValueTask<Result> Handle(SendGuestCommandCommand command, CancellationToken cancellationToken)
        {
            var caller = JamGuards.InJam(store, memberships, command.MemberId);
            if (caller.IsFailure)
                return JamGuards.Done(Result.Failure(caller));

            var jam = caller.Value.Jam;
            if (jam.HostId == command.MemberId)
                return JamGuards.Done(Result.Failure(RelayErrors.NotAllowed, new Dictionary<string, string> { ["reason"] = "the host applies its own commands" }));

            if (CommandKinds.Problem(command.Command) is { } problem)
                return JamGuards.Done(Result.Failure(RelayErrors.InvalidMessage, new Dictionary<string, string> { ["reason"] = problem }));

            if (!CommandKinds.Allowed(jam.GuestControl, command.Command.Kind))
                return JamGuards.Done(Result.Failure(RelayErrors.NotAllowed, new Dictionary<string, string> { ["guestControl"] = $"{jam.GuestControl}" }));

            if (store.FindMember(jam.HostId) is not { Connected: true } host)
                return JamGuards.Done(Result.Failure(RelayErrors.HostOffline));

            notifier.Send(host.Id, new ForwardedCommandDto(command.MemberId, command.Command));
            return JamGuards.Done(Result.Success());
        }
    }
}
