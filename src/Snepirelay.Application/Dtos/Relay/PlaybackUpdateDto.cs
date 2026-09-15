using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Relay
{
    public record PlaybackUpdateDto(PlaybackState State, long ServerTime) : RelayEventDto;
}
