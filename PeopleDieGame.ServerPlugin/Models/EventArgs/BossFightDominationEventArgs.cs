using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class BossFightDominationEventArgs : System.EventArgs
    {
        public Team OldAttackers;
        public Team NewAttackers;

        public BossFightDominationEventArgs(Team oldAttackers, Team newAttackers)
        {
            OldAttackers = oldAttackers ?? throw new ArgumentNullException(nameof(oldAttackers));
            NewAttackers = newAttackers ?? throw new ArgumentNullException(nameof(newAttackers));
        }
    }
}
