using Snepirelay.Domain;

namespace Snepirelay.Infrastructure.Interfaces
{
    public interface IJamStore
    {
        Member? FindMember(string memberId);
        Member? FindMemberByInstall(string installId);
        IReadOnlyList<Member> Members();
        void AddMember(Member member);
        void RemoveMember(Member member);

        Jam? FindJam(string jamId);
        Jam? FindJamByToken(string joinToken);
        void AddJam(Jam jam);
        void RemoveJam(Jam jam);
    }
}
