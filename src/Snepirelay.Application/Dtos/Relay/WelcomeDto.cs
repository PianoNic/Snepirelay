using Snepirelay.Application.Dtos.Jams;
using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Relay
{
    public record WelcomeDto(int Protocol, string MemberId, long ServerTime, JamDto? Session, PlaybackState? Playback) : RelayEventDto;
}
