using Snepirelay.Domain.Enums;

namespace Snepirelay.Domain
{
    public class Jam
    {
        public required string Id { get; init; }
        public required string JoinToken { get; init; }
        public required string HostId { get; init; }
        public List<JamMembership> Members { get; } = [];
        public GuestControl GuestControl { get; set; } = GuestControl.Full;
        public PlaybackState? Playback { get; set; }
        public long Timestamp { get; private set; }

        public bool Has(string memberId) => Members.Exists(m => m.MemberId == memberId);

        public void Touch(DateTimeOffset now) => Timestamp = Math.Max(Timestamp + 1, now.ToUnixTimeMilliseconds());
    }
}
