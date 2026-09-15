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
}
