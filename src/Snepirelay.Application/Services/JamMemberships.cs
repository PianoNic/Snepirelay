using System.Security.Cryptography;
using Snepirelay.Application.Models;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Services
{
    public class JamMemberships(IJamStore store, JamNotifier notifier, TimeProvider time)
    {
        private const string TokenAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public Jam? JamOf(Member member) => member.JamId is { } id ? store.FindJam(id) : null;

        public Jam Create(Member host)
        {
            LeaveCurrent(member: host);

            var jam = new Jam { Id = NewToken(16), JoinToken = UniqueJoinToken(), HostId = host.Id };
            jam.Members.Add(new JamMembership(host.Id, time.GetUtcNow()));
            jam.Touch(time.GetUtcNow());
            store.AddJam(jam);
            host.JamId = jam.Id;
            return jam;
        }

        public void Add(Jam jam, Member member)
        {
            LeaveCurrent(member);

            jam.Members.Add(new JamMembership(member.Id, time.GetUtcNow()));
            jam.Touch(time.GetUtcNow());
            member.JamId = jam.Id;
        }

        public void LeaveCurrent(Member member)
        {
            if (JamOf(member) is not { } jam)
                return;

            if (jam.HostId == member.Id)
                End(jam, hostReason: SessionReasons.YouLeft);
            else
                Remove(jam, member, SessionReasons.YouLeft, SessionReasons.UserLeft);
        }

        public void Remove(Jam jam, Member member, string reasonForMember, string reasonForOthers)
        {
            jam.Members.RemoveAll(m => m.MemberId == member.Id);
            jam.Touch(time.GetUtcNow());
            member.JamId = null;

            notifier.SendGone(member.Id, reasonForMember, [member.Id]);
            notifier.Broadcast(jam, reasonForOthers, [member.Id]);
        }

        public void End(Jam jam, string hostReason = SessionReasons.SessionDeleted)
        {
            store.RemoveJam(jam);

            foreach (var membership in jam.Members)
            {
                if (store.FindMember(membership.MemberId) is { } member)
                    member.JamId = null;

                var reason = membership.MemberId == jam.HostId ? hostReason : SessionReasons.SessionDeleted;
                notifier.SendGone(membership.MemberId, reason, [jam.HostId]);
            }
        }

        private string UniqueJoinToken()
        {
            string token;
            do token = NewToken(12);
            while (store.FindJamByToken(token) is not null);
            return token;
        }

        public static string NewToken(int length) => RandomNumberGenerator.GetString(TokenAlphabet, length);
    }
}
