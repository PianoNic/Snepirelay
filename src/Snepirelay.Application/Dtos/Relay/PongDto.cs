namespace Snepirelay.Application.Dtos.Relay
{
    public record PongDto(long ClientTime, long ServerTime) : RelayEventDto;
}
