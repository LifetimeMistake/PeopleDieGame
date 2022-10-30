using SDG.Unturned;
using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class TeamBaseClaimEventArgs : System.EventArgs
    {
        public Team Team;
        public ClaimBubble ClaimBubble;

        public TeamBaseClaimEventArgs(Team team, ClaimBubble claimBubble)
        {
            Team = team ?? throw new ArgumentNullException(nameof(team));
            ClaimBubble = claimBubble ?? throw new ArgumentNullException(nameof(claimBubble));
        }
    }
}
