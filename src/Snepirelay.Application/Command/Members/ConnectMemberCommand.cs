using System.Security.Cryptography;
using System.Text;
using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.Application.Dtos.Relay;
using Snepirelay.Application.Interfaces;
using Snepirelay.Application.Mappings.Jams;
using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Members
{
    public record ConnectMemberCommand(
        IRelayConnection Connection,
        int Protocol,
        string InstallId,
        string Secret,
        MemberProfile Profile,
        DeviceInfo Device) : ICommand<Result<string>>;

    public class ConnectMemberCommandHandler(
        IJamStore store,
        ConnectionRegistry connections,
        JamMemberships memberships,
        JamNotifier notifier,
        IOptions<RelayOptions> options,
        TimeProvider time) : ICommandHandler<ConnectMemberCommand, Result<string>>
    {
        public ValueTask<Result<string>> Handle(ConnectMemberCommand command, CancellationToken cancellationToken)
        {
            if (command.Protocol != RelayProtocol.Version)
                return Fail(RelayErrors.UnsupportedProtocol, new Dictionary<string, string> { ["supported"] = $"{RelayProtocol.Version}" });

            if (Problem(command) is { } problem)
                return Fail(RelayErrors.InvalidMessage, new Dictionary<string, string> { ["reason"] = problem });

            var secretHash = SHA256.HashData(Encoding.UTF8.GetBytes(command.Secret));
            var now = time.GetUtcNow();
            var member = store.FindMemberByInstall(command.InstallId);

            if (member is not null && !CryptographicOperations.FixedTimeEquals(member.SecretHash, secretHash))
                return Fail(RelayErrors.Unauthorized);

            if (member is null)
            {
                member = new Member
                {
                    Id = JamMemberships.NewToken(16),
                    InstallId = command.InstallId,
                    SecretHash = secretHash,
                    Profile = command.Profile,
                    Device = command.Device,
                };
                store.AddMember(member);
            }

            member.Profile = command.Profile;
            member.Device = command.Device;

            var wasConnected = member.Connected;
            connections.Attach(member.Id, command.Connection)?.Close("replaced");
            member.Connected = true;
            member.LastSeenAt = now;
            member.DisconnectedAt = null;

            var jam = memberships.JamOf(member);
            command.Connection.Send(new WelcomeDto(
                RelayProtocol.Version,
                member.Id,
                now.ToUnixTimeMilliseconds(),
                jam?.ToDto(member.Id, store, options.Value.MaxMemberCount),
                jam is not null && jam.HostId != member.Id ? jam.Playback : null));

            if (jam is not null && !wasConnected)
            {
                jam.Touch(now);
                notifier.Broadcast(jam, SessionReasons.UserUpdated, [member.Id], except: member.Id);
            }

            return ValueTask.FromResult(Result.Success(member.Id));
        }

        private static string? Problem(ConnectMemberCommand command) => command switch
        {
            { InstallId.Length: < 8 or > 128 } => "installId must be 8 to 128 characters",
            { Secret.Length: < 32 or > 256 } => "secret must be 32 to 256 characters",
            { Profile.DisplayName.Length: < 1 or > 100 } => "displayName must be 1 to 100 characters",
            { Profile.ImageUrl.Length: > 2048 } => "imageUrl is too long",
            { Device.DeviceId.Length: < 1 or > 128 } => "deviceId must be 1 to 128 characters",
            { Device.Name.Length: > 100 } => "device name is too long",
            _ => null,
        };

        private static ValueTask<Result<string>> Fail(string error, IReadOnlyDictionary<string, string>? values = null) =>
            ValueTask.FromResult(Result.Failure<string>(error, values));
    }
}
