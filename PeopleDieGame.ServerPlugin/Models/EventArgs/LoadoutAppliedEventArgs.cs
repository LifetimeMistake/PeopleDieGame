using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class LoadoutAppliedEventArgs : System.EventArgs
    {
        public PlayerData Player;
        public Loadout Loadout;

        public LoadoutAppliedEventArgs(PlayerData player, Loadout loadout)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        }
    }
}
