using Snepirelay.Domain;
using Snepirelay.Domain.Enums;

namespace Snepirelay.Application.Models
{
    public static class RelayProtocol
    {
        public const int Version = 1;
    }

    public static class RelayErrors
    {
        public const string HelloRequired = "HELLO_REQUIRED";
        public const string UnsupportedProtocol = "UNSUPPORTED_PROTOCOL";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string InvalidMessage = "INVALID_MESSAGE";
        public const string UnknownType = "UNKNOWN_TYPE";
        public const string MessageTooLarge = "MESSAGE_TOO_LARGE";
        public const string NotInSession = "NOT_IN_SESSION";
        public const string SessionNotFound = "SESSION_NOT_FOUND";
        public const string SessionFull = "SESSION_FULL";
        public const string NotHost = "NOT_HOST";
        public const string NotAllowed = "NOT_ALLOWED";
        public const string MemberNotFound = "MEMBER_NOT_FOUND";
        public const string HostOffline = "HOST_OFFLINE";
    }

    public static class SessionReasons
    {
        public const string YouJoined = "YOU_JOINED";
        public const string UserJoined = "USER_JOINED";
        public const string YouLeft = "YOU_LEFT";
        public const string UserLeft = "USER_LEFT";
        public const string YouWereKicked = "YOU_WERE_KICKED";
        public const string UserKicked = "USER_KICKED";
        public const string SessionDeleted = "SESSION_DELETED";
        public const string UserUpdated = "USER_UPDATED";
        public const string SettingsUpdated = "SETTINGS_UPDATED";
    }

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
