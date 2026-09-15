using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Jams
{
    public record JamMemberDto(
        string Id,
        string? UserId,
        string DisplayName,
        string? ImageUrl,
        DeviceInfo Device,
        bool IsHost,
        bool IsCurrentUser,
        bool Listening,
        long JoinedAt);
}
