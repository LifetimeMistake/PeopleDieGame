using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
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
