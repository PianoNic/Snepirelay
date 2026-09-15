using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Snepirelay.Tests.Support
{
    public sealed class RelayClient(WebSocket socket) : IAsyncDisposable
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        public string InstallId { get; private set; } = $"install-{Guid.NewGuid():N}";
        public string Secret { get; private set; } = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
        public string MemberId { get; private set; } = string.Empty;

        public Task SendAsync(object message) =>
            socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message, Json), WebSocketMessageType.Text, true, CancellationToken.None);

        public Task SendRawAsync(string text) =>
            socket.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, CancellationToken.None);

        public async Task<JsonElement> HelloAsync(string displayName, RelayClient? sameInstallAs = null, string? secret = null, int protocol = 1)
        {
            if (sameInstallAs is not null)
            {
                InstallId = sameInstallAs.InstallId;
                Secret = secret ?? sameInstallAs.Secret;
            }

            await SendAsync(new
            {
                type = "hello",
                protocol,
                installId = InstallId,
                secret = Secret,
                profile = new { userId = $"user-{displayName}", displayName, imageUrl = (string?)null },
                device = new { deviceId = $"device-{displayName}", name = "S25 Ultra", type = "Smartphone" },
            });

            var reply = await ReceiveAsync();
            if (reply.Type() == "welcome")
                MemberId = reply.GetProperty("memberId").GetString()!;

            return reply;
        }

        public async Task<JsonElement> ReceiveAsync()
        {
            using var timeout = new CancellationTokenSource(Timeout);
            var buffer = new byte[64 * 1024];
            using var message = new MemoryStream();

            while (true)
            {
                var received = await socket.ReceiveAsync(buffer, timeout.Token);
                if (received.MessageType == WebSocketMessageType.Close)
                    throw new InvalidOperationException($"closed: {socket.CloseStatusDescription}");

                message.Write(buffer, 0, received.Count);
                if (received.EndOfMessage)
                    return JsonDocument.Parse(message.ToArray()).RootElement.Clone();
            }
        }

        public async Task<JsonElement> ExpectAsync(string type)
        {
            var message = await ReceiveAsync();
            if (message.Type() != type)
                throw new InvalidOperationException($"expected {type}, got {message}");

            return message;
        }

        public async Task<string?> ExpectClosedAsync()
        {
            using var timeout = new CancellationTokenSource(Timeout);
            var buffer = new byte[64 * 1024];

            while (true)
            {
                var received = await socket.ReceiveAsync(buffer, timeout.Token);
                if (received.MessageType == WebSocketMessageType.Close)
                    return socket.CloseStatusDescription;
            }
        }

        public Task CloseAsync() => socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);

        public ValueTask DisposeAsync()
        {
            socket.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    public static class JsonElementExtensions
    {
        public static string? Type(this JsonElement element) => element.GetProperty("type").GetString();

        public static string? Str(this JsonElement element, params string[] path) => element.At(path).GetString();

        public static JsonElement At(this JsonElement element, params string[] path) =>
            path.Aggregate(element, (current, key) => current.GetProperty(key));
    }
}
