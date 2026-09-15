using Snepirelay.Application.Dtos.Jams;

namespace Snepirelay.Application.Dtos.Relay
{
    public record SessionUpdateDto(string Reason, JamDto? Session, IReadOnlyList<string> MemberIds) : RelayEventDto;
}
