using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
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
