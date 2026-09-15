using Snepirelay.Domain;
using Snepirelay.Domain.Enums;

namespace Snepirelay.Application.Models
{
    public static class CommandKinds
    {
        public const string Pause = "pause";
        public const string Resume = "resume";
        public const string Seek = "seek";
        public const string Next = "next";
        public const string Previous = "previous";
        public const string Play = "play";
        public const string AddToQueue = "addToQueue";
        public const string RemoveFromQueue = "removeFromQueue";
        public const string MoveQueue = "moveQueue";

        public static string? Problem(RelayCommand command) => command.Kind switch
        {
            Pause or Resume or Next or Previous => null,
            Seek => command.PositionMs is >= 0 ? null : "seek needs a positionMs of 0 or more",
            Play => command.Uri is not null || command.ContextUri is not null ? null : "play needs a uri or a contextUri",
            AddToQueue => command.Uri is not null ? null : "addToQueue needs a uri",
            RemoveFromQueue => command.Uid is not null || command.Index is >= 0 ? null : "removeFromQueue needs a uid or an index",
            MoveQueue => command.Index is >= 0 && command.ToIndex is >= 0 ? null : "moveQueue needs an index and a toIndex",
            _ => $"unknown command kind '{command.Kind}'",
        };

        public static bool Allowed(GuestControl control, string kind) => control switch
        {
            GuestControl.Full => true,
            GuestControl.QueueOnly => kind == AddToQueue,
            _ => false,
        };
    }
}
