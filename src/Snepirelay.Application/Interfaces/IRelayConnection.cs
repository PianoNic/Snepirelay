using Snepirelay.Application.Dtos.Relay;

namespace Snepirelay.Application.Interfaces
{
    public interface IRelayConnection
    {
        void Send(RelayEventDto message);
        void Close(string reason);
    }
}
