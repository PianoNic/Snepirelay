using Snepirelay.Application.Dtos.Jams;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Mappings.Jams
{
    public static class JamMappings
    {
        public static JamDto ToDto(this Jam jam, string viewerId, IJamStore store, int maxMemberCount) => new(
            SessionId: jam.Id,
            JoinToken: jam.JoinToken,
            OwnerId: jam.HostId,
            IsOwner: jam.HostId == viewerId,
            Members: [.. jam.Members
                .Select(m => (Membership: m, Member: store.FindMember(m.MemberId)))
                .Where(x => x.Member is not null)
                .Select(x => x.Member!.ToDto(jam, x.Membership, viewerId))],
            GuestControl: jam.GuestControl,
            MaxMemberCount: maxMemberCount,
            Timestamp: jam.Timestamp);

        public static JamPreviewDto ToPreviewDto(this Jam jam, IJamStore store, int maxMemberCount)
        {
            var host = store.FindMember(jam.HostId);
            return new JamPreviewDto(host?.Profile.DisplayName ?? string.Empty, host?.Profile.ImageUrl, jam.Members.Count, maxMemberCount);
        }

        private static JamMemberDto ToDto(this Member member, Jam jam, JamMembership membership, string viewerId) => new(
            Id: member.Id,
            UserId: member.Profile.UserId,
            DisplayName: member.Profile.DisplayName,
            ImageUrl: member.Profile.ImageUrl,
            Device: member.Device,
            IsHost: member.Id == jam.HostId,
            IsCurrentUser: member.Id == viewerId,
            Listening: member.Connected,
            JoinedAt: membership.JoinedAt.ToUnixTimeMilliseconds());
    }
}
