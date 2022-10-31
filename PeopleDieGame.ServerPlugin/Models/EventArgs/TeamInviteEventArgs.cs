using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class TeamInviteEventArgs : System.EventArgs
    {
        public TeamInvite Invite;

        public TeamInviteEventArgs(TeamInvite invite)
        {
            Invite = invite ?? throw new ArgumentNullException(nameof(invite));
        }
    }
}
