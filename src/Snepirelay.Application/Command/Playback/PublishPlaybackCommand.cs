using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.Application.Command.Jams;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Playback
{
    public record PublishPlaybackCommand(string MemberId, PlaybackState State) : ICommand<Result>;

    public class PublishPlaybackCommandHandler(
        IJamStore store,
        JamMemberships memberships,
        JamNotifier notifier,
        IOptions<RelayOptions> options) : ICommandHandler<PublishPlaybackCommand, Result>
    {
        public ValueTask<Result> Handle(PublishPlaybackCommand command, CancellationToken cancellationToken)
        {
            var caller = JamGuards.InJam(store, memberships, command.MemberId, hostOnly: true);
            if (caller.IsFailure)
                return JamGuards.Done(Result.Failure(caller));

            var maxQueue = options.Value.MaxQueueLength;
            if (command.State.Queue.Count > maxQueue || command.State.PositionMs < 0 || command.State.DurationMs < 0)
                return JamGuards.Done(Result.Failure(RelayErrors.InvalidMessage, new Dictionary<string, string> { ["maxQueueLength"] = $"{maxQueue}" }));

            var now = notifier.Now;
            var skew = (long)options.Value.MaxClockSkew.TotalMilliseconds;
            var sampledAt = command.State.SampledAt is { } sampled && Math.Abs(sampled - now) <= skew ? sampled : now;

            var jam = caller.Value.Jam;
            jam.Playback = command.State with { SampledAt = sampledAt };

            foreach (var membership in jam.Members)
                notifier.SendPlayback(jam, membership.MemberId);

            return JamGuards.Done(Result.Success());
        }
    }
}
