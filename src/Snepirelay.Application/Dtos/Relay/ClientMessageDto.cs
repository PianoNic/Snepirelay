using System.Text.Json.Serialization;
using Snepirelay.Domain;
using Snepirelay.Domain.Enums;

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

    public record HelloMessageDto(int Protocol, string InstallId, string Secret, MemberProfile Profile, DeviceInfo Device) : ClientMessageDto;

    public record PingMessageDto(long ClientTime) : ClientMessageDto;

    public record CreateMessageDto : ClientMessageDto;

    public record JoinMessageDto(string JoinToken) : ClientMessageDto;

    public record LeaveMessageDto : ClientMessageDto;

    public record EndMessageDto : ClientMessageDto;

    public record KickMessageDto(string MemberId) : ClientMessageDto;

    public record SettingsMessageDto(GuestControl GuestControl) : ClientMessageDto;

    public record PlaybackMessageDto(PlaybackState State) : ClientMessageDto;

    public record CommandMessageDto(RelayCommand Command) : ClientMessageDto;
}
