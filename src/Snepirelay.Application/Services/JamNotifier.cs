using Microsoft.Extensions.Options;
using Snepirelay.Application.Dtos.Relay;
using Snepirelay.Application.Mappings.Jams;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Services
{
    public class JamNotifier(IJamStore store, ConnectionRegistry connections, IOptions<RelayOptions> options, TimeProvider time)
    {
        public long Now => time.GetUtcNow().ToUnixTimeMilliseconds();

        public void Send(string memberId, RelayEventDto message) => connections.Of(memberId)?.Send(message);

        public void SendSession(Jam jam, string memberId, string reason, IReadOnlyList<string> about) =>
            Send(memberId, new SessionUpdateDto(reason, jam.ToDto(memberId, store, options.Value.MaxMemberCount), about));

        public void SendGone(string memberId, string reason, IReadOnlyList<string> about) =>
            Send(memberId, new SessionUpdateDto(reason, null, about));

        public void Broadcast(Jam jam, string reason, IReadOnlyList<string> about, string? except = null)
        {
            foreach (var membership in jam.Members.Where(m => m.MemberId != except))
                SendSession(jam, membership.MemberId, reason, about);
        }

        public void SendPlayback(Jam jam, string memberId)
        {
            if (jam.Playback is { } playback && memberId != jam.HostId)
                Send(memberId, new PlaybackUpdateDto(playback, Now));
        }
    }
}
