using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class LoadoutEventArgs : System.EventArgs
    {
        public Loadout Loadout;

        public LoadoutEventArgs(Loadout loadout)
        {
            Loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        }
    }
}
