using Snepirelay.Domain.Enums;

namespace Snepirelay.Domain
{
    public record MemberProfile(string? UserId, string DisplayName, string? ImageUrl);

    public record DeviceInfo(string DeviceId, string Name, string Type);

    public record QueueEntry(string Uri, string? Uid = null, string? AddedBy = null);

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

    public record RelayCommand(
        string Kind,
        long? PositionMs = null,
        string? Uri = null,
        string? ContextUri = null,
        string? Uid = null,
        int? Index = null,
        int? ToIndex = null);
}
