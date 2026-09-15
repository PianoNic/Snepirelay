using Snepirelay.Domain;

namespace Snepirelay.Application.Dtos.Relay
{
    public record HelloMessageDto(int Protocol, string InstallId, string Secret, MemberProfile Profile, DeviceInfo Device) : ClientMessageDto;
}
