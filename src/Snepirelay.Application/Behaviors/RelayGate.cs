namespace Snepirelay.Application.Behaviors
{
    public sealed class RelayGate
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
    }
}
