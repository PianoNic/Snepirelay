namespace Snepirelay.Application
{
    public class RelayOptions
    {
        public const string Section = "Relay";

        public int MaxMemberCount { get; set; } = 32;
        public int MaxQueueLength { get; set; } = 500;
        public TimeSpan ReconnectGrace { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan IdentityTtl { get; set; } = TimeSpan.FromDays(1);
        public TimeSpan SweepInterval { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan MaxClockSkew { get; set; } = TimeSpan.FromSeconds(10);
        public int MaxMessageBytes { get; set; } = 64 * 1024;
        public int OutboundQueueSize { get; set; } = 256;
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(20);
    }
}
