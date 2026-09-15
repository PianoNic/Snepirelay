using Snepirelay.Application.Models;
using Snepirelay.Application.Services;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Command.Jams
{
    public static class JamGuards
    {
        public static Result<(Member Member, Jam Jam)> InJam(IJamStore store, JamMemberships memberships, string memberId, bool hostOnly = false)
        {
            if (store.FindMember(memberId) is not { } member)
                return Result.Failure<(Member, Jam)>(RelayErrors.HelloRequired);

            if (memberships.JamOf(member) is not { } jam)
                return Result.Failure<(Member, Jam)>(RelayErrors.NotInSession);

            if (hostOnly && jam.HostId != member.Id)
                return Result.Failure<(Member, Jam)>(RelayErrors.NotHost);

            return Result.Success((member, jam));
        }

        public static ValueTask<Result> Done(Result result) => ValueTask.FromResult(result);
    }
}
