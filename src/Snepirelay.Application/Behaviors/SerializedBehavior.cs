using Mediator;

namespace Snepirelay.Application.Behaviors
{
    public sealed class RelayGate
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
    }

    public sealed class SerializedBehavior<TMessage, TResponse>(RelayGate gate) : IPipelineBehavior<TMessage, TResponse>
        where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
        {
            await gate.Semaphore.WaitAsync(cancellationToken);
            try
            {
                return await next(message, cancellationToken);
            }
            finally
            {
                gate.Semaphore.Release();
            }
        }
    }
}
