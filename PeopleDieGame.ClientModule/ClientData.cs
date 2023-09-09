using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.RPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ClientModule
{
    public class ClientData
    {
        public PlayerInfo? PlayerInfo { get; set; }
        public TeamInfo? TeamInfo { get; set; }

        public ClientData() 
        {
            PlayerInfo = null;
            TeamInfo = null;
            
        }
    }
}
