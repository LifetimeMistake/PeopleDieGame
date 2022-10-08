using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class AltarSubmitEventArgs : System.EventArgs
    {
        public Team Team;

        public AltarSubmitEventArgs(Team team)
        {
            Team = team;
        }
    }
}
