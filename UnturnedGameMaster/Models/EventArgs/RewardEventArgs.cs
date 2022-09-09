using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class RewardEventArgs : System.EventArgs
    {
        public PlayerData Player { get; set; }
        public double Reward { get; set; }

        public RewardEventArgs(PlayerData player, double reward)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Reward = reward;
        }
    }
}
