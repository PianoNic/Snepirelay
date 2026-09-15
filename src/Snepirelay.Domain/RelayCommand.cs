namespace Snepirelay.Domain
{
    public record RelayCommand(
        string Kind,
        long? PositionMs = null,
        string? Uri = null,
        string? ContextUri = null,
        string? Uid = null,
        int? Index = null,
        int? ToIndex = null);
}
