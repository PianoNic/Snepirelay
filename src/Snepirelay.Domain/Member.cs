namespace Snepirelay.Domain
{
    public class Member
    {
        public required string Id { get; init; }
        public required string InstallId { get; init; }
        public required byte[] SecretHash { get; init; }
        public required MemberProfile Profile { get; set; }
        public required DeviceInfo Device { get; set; }
        public string? JamId { get; set; }
        public bool Connected { get; set; }
        public DateTimeOffset LastSeenAt { get; set; }
        public DateTimeOffset? DisconnectedAt { get; set; }
    }
}
