using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Relay
{
    public record CommandMessageDto(RelayCommand Command) : ClientMessageDto;
}
