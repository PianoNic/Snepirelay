using System.Collections.Concurrent;
using Snepirelay.Domain;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Infrastructure.Services
{
    public class InMemoryJamStore : IJamStore
    {
        private readonly ConcurrentDictionary<string, Member> _members = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, string> _memberByInstall = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, Jam> _jams = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, string> _jamByToken = new(StringComparer.Ordinal);

        public Member? FindMember(string memberId) => _members.GetValueOrDefault(memberId);

        public Member? FindMemberByInstall(string installId) =>
            _memberByInstall.TryGetValue(installId, out var memberId) ? FindMember(memberId) : null;

        public IReadOnlyList<Member> Members() => [.. _members.Values];

        public void AddMember(Member member)
        {
            _members[member.Id] = member;
            _memberByInstall[member.InstallId] = member.Id;
        }

        public void RemoveMember(Member member)
        {
            _members.TryRemove(member.Id, out _);
            _memberByInstall.TryRemove(member.InstallId, out _);
        }

        public Jam? FindJam(string jamId) => _jams.GetValueOrDefault(jamId);

        public Jam? FindJamByToken(string joinToken) =>
            _jamByToken.TryGetValue(joinToken, out var jamId) ? FindJam(jamId) : null;

        public void AddJam(Jam jam)
        {
            _jams[jam.Id] = jam;
            _jamByToken[jam.JoinToken] = jam.Id;
        }

        public void RemoveJam(Jam jam)
        {
            _jams.TryRemove(jam.Id, out _);
            _jamByToken.TryRemove(jam.JoinToken, out _);
        }
    }
}
