namespace Snepirelay.Application.Dtos.Relay
{
    public record ErrorDto(string Error, IReadOnlyDictionary<string, string>? Values) : RelayEventDto;
}
