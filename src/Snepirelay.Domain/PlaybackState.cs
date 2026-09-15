using Snepirelay.Domain.Enums;

namespace Snepirelay.Domain
{
    public record PlaybackState(
        string? TrackUri,
        string? ContextUri,
        long PositionMs,
        long DurationMs,
        bool Paused,
        bool Shuffle,
        RepeatMode Repeat,
        IReadOnlyList<QueueEntry> Queue,
        long? SampledAt = null);
}
