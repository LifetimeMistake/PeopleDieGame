using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class PlayerEventArgs : System.EventArgs
    {
        public PlayerData Player;

        public PlayerEventArgs(PlayerData player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }
    }
}
