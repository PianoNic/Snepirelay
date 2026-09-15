using Snepirelay.Domain.Enums;

namespace Snepirelay.Application.Dtos.Relay
{
    public record SettingsMessageDto(GuestControl GuestControl) : ClientMessageDto;
}
