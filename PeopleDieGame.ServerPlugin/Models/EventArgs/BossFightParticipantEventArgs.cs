using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class BossFightParticipantEventArgs
    {
        public BossFight BossFight;
        public UnturnedPlayer Player;

        public BossFightParticipantEventArgs(BossFight bossFight, UnturnedPlayer player)
        {
            BossFight = bossFight ?? throw new ArgumentNullException(nameof(bossFight));
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }
    }
}
