using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.Application.Command.Jams;
using Snepirelay.Application.Command.Members;
using Snepirelay.Application.Command.Playback;
using Snepirelay.Application.Dtos.Relay;
using Snepirelay.Application.Models;

namespace Snepirelay.Application.Services
{
    public class RelaySocketRunner(IMediator mediator, IOptions<RelayOptions> options, TimeProvider time)
    {
        public async Task RunAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            var settings = options.Value;
            var connection = new WebSocketRelayConnection(socket, settings.OutboundQueueSize);
            var writer = connection.RunWriterAsync();
            string? memberId = null;

            try
            {
                using var receiveCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connection.Aborted);

                while (await ReceiveAsync(socket, settings.MaxMessageBytes, receiveCancellation.Token) is { } frame)
                {
                    if (frame.TooLarge)
                    {
                        connection.Send(Error(RelayErrors.MessageTooLarge, null, ("maxMessageBytes", $"{settings.MaxMessageBytes}")));
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
                        if (message is not HelloMessageDto hello)
                        {
                            connection.Send(Error(RelayErrors.HelloRequired, message.Rid));
                            connection.Close("hello_required");
                            break;
                        }

                        var connected = await mediator.Send(
                            new ConnectMemberCommand(connection, hello.Protocol, hello.InstallId, hello.Secret, hello.Profile, hello.Device),
                            cancellationToken);

                        if (connected.IsFailure)
                        {
                            connection.Send(Result.Failure(connected).ToRelayReply(hello.Rid)!);
                            connection.Close(connected.Error!.ToLowerInvariant());
                            break;
                        }

                        memberId = connected.Value;
                        continue;
                    }

                    if (message is PingMessageDto ping)
                    {
                        connection.Send(new PongDto(ping.ClientTime, time.GetUtcNow().ToUnixTimeMilliseconds()) { Rid = ping.Rid });
                        continue;
                    }

                    var result = await DispatchAsync(memberId, message, cancellationToken);
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

                connection.Close(WebSocketRelayConnection.NormalReason);
                await writer;
            }
        }

        private ValueTask<Result> DispatchAsync(string memberId, ClientMessageDto message, CancellationToken cancellationToken) =>
            message switch
            {
                CreateMessageDto => mediator.Send(new CreateJamCommand(memberId), cancellationToken),
                JoinMessageDto join => mediator.Send(new JoinJamCommand(memberId, join.JoinToken), cancellationToken),
                LeaveMessageDto => mediator.Send(new LeaveJamCommand(memberId), cancellationToken),
                EndMessageDto => mediator.Send(new EndJamCommand(memberId), cancellationToken),
                KickMessageDto kick => mediator.Send(new KickMemberCommand(memberId, kick.MemberId), cancellationToken),
                SettingsMessageDto settings => mediator.Send(new SetGuestControlCommand(memberId, settings.GuestControl), cancellationToken),
                PlaybackMessageDto playback => mediator.Send(new PublishPlaybackCommand(memberId, playback.State), cancellationToken),
                CommandMessageDto command => mediator.Send(new SendGuestCommandCommand(memberId, command.Command), cancellationToken),
                _ => ValueTask.FromResult(Result.Failure(RelayErrors.InvalidMessage, new Dictionary<string, string> { ["reason"] = "already said hello" })),
            };

        private static (ClientMessageDto? Message, ErrorDto? Error) Parse(string text)
        {
            try
            {
                using var document = JsonDocument.Parse(text);
                var root = document.RootElement;
                var type = StringProperty(root, "type");
                var rid = StringProperty(root, "rid");

                if (type is null)
                    return (null, Error(RelayErrors.InvalidMessage, rid, ("reason", "a message needs a string type")));

                if (!ClientMessageDto.Types.Contains(type))
                    return (null, Error(RelayErrors.UnknownType, rid, ("type", type)));

                try
                {
                    return (root.Deserialize<ClientMessageDto>(RelayJson.Options), null);
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

        private static string? StringProperty(JsonElement root, string name) =>
            root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;

        private static ErrorDto Error(string code, string? rid, params (string Key, string Value)[] values) =>
            new(code, values.Length == 0 ? null : values.ToDictionary(v => v.Key, v => v.Value)) { Rid = rid };

        private static async Task<(string Text, bool TooLarge)?> ReceiveAsync(WebSocket socket, int maxBytes, CancellationToken cancellationToken)
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
                        return (string.Empty, true);

                    message.Write(buffer, 0, received.Count);

                    if (received.EndOfMessage)
                        return (Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length), false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
