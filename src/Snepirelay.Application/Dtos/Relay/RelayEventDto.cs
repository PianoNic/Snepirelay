using System.Text.Json.Serialization;
using Snepirelay.Application.Dtos.Jams;
using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Relay
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(WelcomeDto), "welcome")]
    [JsonDerivedType(typeof(PongDto), "pong")]
    [JsonDerivedType(typeof(SessionUpdateDto), "session_update")]
    [JsonDerivedType(typeof(PlaybackUpdateDto), "playback")]
    [JsonDerivedType(typeof(ForwardedCommandDto), "command")]
    [JsonDerivedType(typeof(OkDto), "ok")]
    [JsonDerivedType(typeof(ErrorDto), "error")]
    public abstract record RelayEventDto
    {
        public string? Rid { get; init; }
    }

    public record WelcomeDto(int Protocol, string MemberId, long ServerTime, JamDto? Session, PlaybackState? Playback) : RelayEventDto;

    public record PongDto(long ClientTime, long ServerTime) : RelayEventDto;

    public record SessionUpdateDto(string Reason, JamDto? Session, IReadOnlyList<string> MemberIds) : RelayEventDto;

    public record PlaybackUpdateDto(PlaybackState State, long ServerTime) : RelayEventDto;

    public record ForwardedCommandDto(string From, RelayCommand Command) : RelayEventDto;

    public record OkDto : RelayEventDto;

    public record ErrorDto(string Error, IReadOnlyDictionary<string, string>? Values) : RelayEventDto;
}
