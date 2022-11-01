using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Models.EventArgs
{
    public class InviteResponseEventArgs : System.EventArgs
    {
        public SteamPlayer Player;

        public InviteResponseEventArgs(SteamPlayer player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }
    }
}
