using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Models.EventArgs
{
    public class UpdatePlayerInfoEventArgs : System.EventArgs
    {
        public PlayerInfo PlayerInfo { get; set; }

        public UpdatePlayerInfoEventArgs(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }
    }
}
