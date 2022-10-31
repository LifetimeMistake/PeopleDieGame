using System;

namespace PeopleDieGame.ServerPlugin.Models
{
    public class TeamInvite
    {
        public PlayerData Target;
        public PlayerData Inviter;
        public Team Team;
        public DateTime InviteDate;
        public TimeSpan InviteTTL;

        public TeamInvite(PlayerData target, PlayerData inviter, Team team, DateTime inviteDate, TimeSpan inviteTTL)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Inviter = inviter ?? throw new ArgumentNullException(nameof(inviter));
            Team = team ?? throw new ArgumentNullException(nameof(team));
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
