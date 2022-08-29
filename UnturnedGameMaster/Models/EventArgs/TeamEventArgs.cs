using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class TeamEventArgs : System.EventArgs
    {
        public Team Team;

        public TeamEventArgs(Team team)
        {
            Team = team ?? throw new ArgumentNullException(nameof(team));
        }
    }
}
