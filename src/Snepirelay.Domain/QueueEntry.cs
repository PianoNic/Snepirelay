namespace Snepirelay.Domain
{
    public record QueueEntry(string Uri, string? Uid = null, string? AddedBy = null);
}
