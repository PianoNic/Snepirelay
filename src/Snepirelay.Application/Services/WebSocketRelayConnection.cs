using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Snepirelay.Application.Dtos.Relay;
using Snepirelay.Application.Interfaces;

namespace Snepirelay.Application.Services
{
    public sealed class WebSocketRelayConnection(WebSocket socket, int queueSize) : IRelayConnection
    {
        public const string NormalReason = "closed";

        private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);

        private readonly Channel<RelayEventDto> _outbound = Channel.CreateBounded<RelayEventDto>(
            new BoundedChannelOptions(queueSize) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });

        private readonly CancellationTokenSource _abort = new();
        private string? _closeReason;

        public CancellationToken Aborted => _abort.Token;

        public void Send(RelayEventDto message)
        {
            if (!_outbound.Writer.TryWrite(message))
                Close("slow_consumer");
        }

        public void Close(string reason)
        {
            Interlocked.CompareExchange(ref _closeReason, reason, null);
            _outbound.Writer.TryComplete();
        }

        public async Task RunWriterAsync()
        {
            try
            {
                await foreach (var message in _outbound.Reader.ReadAllAsync())
                {
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(message, RelayJson.Options);
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                }

                if (_closeReason is { } reason && socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    _abort.CancelAfter(CloseTimeout);
                    var status = reason == NormalReason ? WebSocketCloseStatus.NormalClosure : WebSocketCloseStatus.PolicyViolation;
                    await socket.CloseOutputAsync(status, reason, CancellationToken.None);
                }
            }
            catch (WebSocketException)
            {
                _abort.Cancel();
            }
        }
    }
}
