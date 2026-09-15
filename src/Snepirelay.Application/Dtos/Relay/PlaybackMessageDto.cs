using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Relay
{
    public record PlaybackMessageDto(PlaybackState State) : ClientMessageDto;
}
