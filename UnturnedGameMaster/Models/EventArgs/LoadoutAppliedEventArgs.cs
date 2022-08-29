using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class LoadoutAppliedEventArgs : System.EventArgs
    {
        public UnturnedPlayer Player;
        public Loadout Loadout;

        public LoadoutAppliedEventArgs(UnturnedPlayer player, Loadout loadout)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        }
    }
}
