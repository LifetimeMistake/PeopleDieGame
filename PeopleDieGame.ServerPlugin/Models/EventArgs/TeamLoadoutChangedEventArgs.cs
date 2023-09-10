using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class TeamLoadoutChangedEventArgs : System.EventArgs
    {
        public Team Team;
        public Loadout Loadout;

        public TeamLoadoutChangedEventArgs(Team team, Loadout loadout)
        {
            Team = team ?? throw new ArgumentNullException(nameof(team));
            Loadout = loadout;
        }
    }
}
