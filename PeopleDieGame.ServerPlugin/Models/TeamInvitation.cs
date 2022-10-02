using System;

namespace PeopleDieGame.ServerPlugin.Models
{
    public class TeamInvitation
    {
        public ulong InviterId;
        public ulong TargetId;
        public DateTime InviteDate;
        public TimeSpan InviteTTL;

        public TeamInvitation(ulong inviterId, ulong targetId, DateTime inviteDate, TimeSpan inviteTTL)
        {
            InviterId = inviterId;
            TargetId = targetId;
            InviteDate = inviteDate;
            InviteTTL = inviteTTL;
        }

        public bool IsExpired()
        {
            return (InviteDate + InviteTTL) < DateTime.Now;
        }

        public TimeSpan GetTimeRemaining()
        {
            return (InviteDate + InviteTTL) - DateTime.Now;
        }
    }
}
