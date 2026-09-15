using System.Collections.Concurrent;
using Snepirelay.Application.Interfaces;

namespace Snepirelay.Application.Services
{
    public class ConnectionRegistry
    {
        private readonly ConcurrentDictionary<string, IRelayConnection> _connections = new(StringComparer.Ordinal);

        public IRelayConnection? Of(string memberId) => _connections.GetValueOrDefault(memberId);

        public IRelayConnection? Attach(string memberId, IRelayConnection connection)
        {
            var previous = Of(memberId);
            _connections[memberId] = connection;
            return ReferenceEquals(previous, connection) ? null : previous;
        }

        public bool Detach(string memberId, IRelayConnection connection) =>
            _connections.TryRemove(new KeyValuePair<string, IRelayConnection>(memberId, connection));
    }
}
