using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.Application;
using Snepirelay.Application.Command.Jams;
using Snepirelay.Application.Command.Members;
using Snepirelay.Application.Command.Playback;
using Snepirelay.Application.Dtos.Relay;
using Snepirelay.Application.Models;

namespace Snepirelay.API.Relay
{
    public static class RelaySocketEndpoint
    {
        public static async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            var mediator = context.RequestServices.GetRequiredService<IMediator>();
            var options = context.RequestServices.GetRequiredService<IOptions<RelayOptions>>().Value;
            var time = context.RequestServices.GetRequiredService<TimeProvider>();

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            var connection = new WebSocketConnection(socket, options.OutboundQueueSize);
            var writer = connection.RunWriterAsync();
            string? memberId = null;

            try
            {
                using var receiveCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, connection.Aborted);

                while (await ReceiveAsync(socket, options.MaxMessageBytes, receiveCancellation.Token) is { } frame)
                {
                    if (frame.TooLarge)
                    {
                        connection.Send(Error(RelayErrors.MessageTooLarge, null, ("maxMessageBytes", $"{options.MaxMessageBytes}")));
                        connection.Close("message_too_large");
                        break;
                    }

                    var parsed = Parse(frame.Text);
                    if (parsed.Error is { } parseError)
                    {
                        connection.Send(parseError);
                        continue;
                    }

                    var message = parsed.Message!;

                    if (memberId is null)
                    {
                        if (message is not HelloMessage hello)
                        {
                            connection.Send(Error(RelayErrors.HelloRequired, message.Rid));
                            connection.Close("hello_required");
                            break;
                        }

                        var connected = await mediator.Send(
                            new ConnectMemberCommand(connection, hello.Protocol, hello.InstallId, hello.Secret, hello.Profile, hello.Device),
                            context.RequestAborted);

                        if (connected.IsFailure)
                        {
                            connection.Send(Result.Failure(connected).ToRelayReply(hello.Rid)!);
                            connection.Close(connected.Error!.ToLowerInvariant());
                            break;
                        }

                        memberId = connected.Value;
                        continue;
                    }

                    if (message is PingMessage ping)
                    {
                        connection.Send(new PongDto(ping.ClientTime, time.GetUtcNow().ToUnixTimeMilliseconds()) { Rid = ping.Rid });
                        continue;
                    }

                    var result = await DispatchAsync(mediator, memberId, message, context.RequestAborted);
                    if (result.ToRelayReply(message.Rid) is { } reply)
                        connection.Send(reply);
                }
            }
            catch (Exception exception) when (exception is WebSocketException or OperationCanceledException)
            {
            }
            finally
            {
                if (memberId is not null)
                    await mediator.Send(new DisconnectMemberCommand(memberId, connection), CancellationToken.None);

                connection.Close(WebSocketConnection.NormalReason);
                await writer;
            }
        }

        private static ValueTask<Result> DispatchAsync(IMediator mediator, string memberId, ClientMessage message, CancellationToken cancellationToken) =>
            message switch
            {
                CreateMessage => mediator.Send(new CreateJamCommand(memberId), cancellationToken),
                JoinMessage join => mediator.Send(new JoinJamCommand(memberId, join.JoinToken), cancellationToken),
                LeaveMessage => mediator.Send(new LeaveJamCommand(memberId), cancellationToken),
                EndMessage => mediator.Send(new EndJamCommand(memberId), cancellationToken),
                KickMessage kick => mediator.Send(new KickMemberCommand(memberId, kick.MemberId), cancellationToken),
                SettingsMessage settings => mediator.Send(new SetGuestControlCommand(memberId, settings.GuestControl), cancellationToken),
                PlaybackMessage playback => mediator.Send(new PublishPlaybackCommand(memberId, playback.State), cancellationToken),
                CommandMessage command => mediator.Send(new SendGuestCommandCommand(memberId, command.Command), cancellationToken),
                _ => ValueTask.FromResult(Result.Failure(RelayErrors.InvalidMessage, new Dictionary<string, string> { ["reason"] = "already said hello" })),
            };

        private static (ClientMessage? Message, ErrorDto? Error) Parse(string text)
        {
            try
            {
                using var document = JsonDocument.Parse(text);
                var root = document.RootElement;
                var type = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String
                    ? typeElement.GetString()
                    : null;
                var rid = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("rid", out var ridElement) && ridElement.ValueKind == JsonValueKind.String
                    ? ridElement.GetString()
                    : null;

                if (type is null)
                    return (null, Error(RelayErrors.InvalidMessage, rid, ("reason", "a message needs a string type")));

                if (!ClientMessage.Types.Contains(type))
                    return (null, Error(RelayErrors.UnknownType, rid, ("type", type)));

                try
                {
                    return (root.Deserialize<ClientMessage>(RelayJson.Options), null);
                }
                catch (JsonException exception)
                {
                    return (null, Error(RelayErrors.InvalidMessage, rid, ("reason", exception.Message)));
                }
            }
            catch (JsonException)
            {
                return (null, Error(RelayErrors.InvalidMessage, null, ("reason", "not JSON")));
            }
        }

        private static ErrorDto Error(string code, string? rid, params (string Key, string Value)[] values) =>
            new(code, values.Length == 0 ? null : values.ToDictionary(v => v.Key, v => v.Value)) { Rid = rid };

        private sealed record Frame(string Text, bool TooLarge);

        private static async Task<Frame?> ReceiveAsync(WebSocket socket, int maxBytes, CancellationToken cancellationToken)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                using var message = new MemoryStream();
                while (true)
                {
                    var received = await socket.ReceiveAsync(buffer, cancellationToken);
                    if (received.MessageType == WebSocketMessageType.Close)
                        return null;

                    if (message.Length + received.Count > maxBytes)
                        return new Frame(string.Empty, TooLarge: true);

                    message.Write(buffer, 0, received.Count);

                    if (received.EndOfMessage)
                        return new Frame(Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length), TooLarge: false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
