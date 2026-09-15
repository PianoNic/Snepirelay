using System.Text.Json.Serialization;

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
}
