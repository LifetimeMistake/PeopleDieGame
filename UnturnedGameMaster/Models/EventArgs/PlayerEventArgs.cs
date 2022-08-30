using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
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
