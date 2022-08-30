using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class TeamMembershipEventArgs : System.EventArgs
    {
        public PlayerData Player;
        public Team Team;

        public TeamMembershipEventArgs(PlayerData player, Team team)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Team = team ?? throw new ArgumentNullException(nameof(team));
        }
    }
}
