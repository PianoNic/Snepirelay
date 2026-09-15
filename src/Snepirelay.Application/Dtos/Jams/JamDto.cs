using Snepirelay.Domain;
using Snepirelay.Domain.Enums;

namespace Snepirelay.Application.Dtos.Jams
{
    public record JamDto(
        string SessionId,
        string JoinToken,
        string OwnerId,
        bool IsOwner,
        IReadOnlyList<JamMemberDto> Members,
        GuestControl GuestControl,
        int MaxMemberCount,
        long Timestamp);

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

    public record JamPreviewDto(string HostDisplayName, string? HostImageUrl, int MemberCount, int MaxMemberCount);
}
