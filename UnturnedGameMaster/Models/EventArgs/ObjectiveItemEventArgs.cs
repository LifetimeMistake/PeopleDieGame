using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class ObjectiveItemEventArgs : System.EventArgs
    {
        public ObjectiveItem ObjectiveItem;

        public ObjectiveItemEventArgs(ObjectiveItem objectiveItem)
        {
            ObjectiveItem = objectiveItem ?? throw new ArgumentNullException(nameof(objectiveItem));
        }
    }
}
