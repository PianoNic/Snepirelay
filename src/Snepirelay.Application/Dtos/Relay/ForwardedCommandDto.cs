using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Relay
{
    public record ForwardedCommandDto(string From, RelayCommand Command) : RelayEventDto;
}
