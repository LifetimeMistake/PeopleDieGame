using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class BossFightEventArgs : System.EventArgs
    {
        public BossFight BossFight;

        public BossFightEventArgs(BossFight bossFight)
        {
            BossFight = bossFight ?? throw new ArgumentNullException(nameof(bossFight));
        }
    }
}
