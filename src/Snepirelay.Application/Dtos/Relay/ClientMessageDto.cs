using System.Text.Json.Serialization;

namespace Snepirelay.Application.Dtos.Relay
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(HelloMessageDto), "hello")]
    [JsonDerivedType(typeof(PingMessageDto), "ping")]
    [JsonDerivedType(typeof(CreateMessageDto), "create")]
    [JsonDerivedType(typeof(JoinMessageDto), "join")]
    [JsonDerivedType(typeof(LeaveMessageDto), "leave")]
    [JsonDerivedType(typeof(EndMessageDto), "end")]
    [JsonDerivedType(typeof(KickMessageDto), "kick")]
    [JsonDerivedType(typeof(SettingsMessageDto), "settings")]
    [JsonDerivedType(typeof(PlaybackMessageDto), "playback")]
    [JsonDerivedType(typeof(CommandMessageDto), "command")]
    public abstract record ClientMessageDto
    {
        public static readonly IReadOnlySet<string> Types = new HashSet<string>(StringComparer.Ordinal)
        {
            "hello", "ping", "create", "join", "leave", "end", "kick", "settings", "playback", "command",
        };

        public string? Rid { get; init; }
    }
}
