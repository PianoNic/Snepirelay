using System.Text.Json.Serialization;
using Snepirelay.Domain;
using Snepirelay.Domain.Enums;

namespace Snepirelay.API.Relay
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(HelloMessage), "hello")]
    [JsonDerivedType(typeof(PingMessage), "ping")]
    [JsonDerivedType(typeof(CreateMessage), "create")]
    [JsonDerivedType(typeof(JoinMessage), "join")]
    [JsonDerivedType(typeof(LeaveMessage), "leave")]
    [JsonDerivedType(typeof(EndMessage), "end")]
    [JsonDerivedType(typeof(KickMessage), "kick")]
    [JsonDerivedType(typeof(SettingsMessage), "settings")]
    [JsonDerivedType(typeof(PlaybackMessage), "playback")]
    [JsonDerivedType(typeof(CommandMessage), "command")]
    public abstract record ClientMessage
    {
        public static readonly IReadOnlySet<string> Types = new HashSet<string>(StringComparer.Ordinal)
        {
            "hello", "ping", "create", "join", "leave", "end", "kick", "settings", "playback", "command",
        };

        public string? Rid { get; init; }
    }

    public record HelloMessage(int Protocol, string InstallId, string Secret, MemberProfile Profile, DeviceInfo Device) : ClientMessage;

    public record PingMessage(long ClientTime) : ClientMessage;

    public record CreateMessage : ClientMessage;

    public record JoinMessage(string JoinToken) : ClientMessage;

    public record LeaveMessage : ClientMessage;

    public record EndMessage : ClientMessage;

    public record KickMessage(string MemberId) : ClientMessage;

    public record SettingsMessage(GuestControl GuestControl) : ClientMessage;

    public record PlaybackMessage(PlaybackState State) : ClientMessage;

    public record CommandMessage(RelayCommand Command) : ClientMessage;
}
