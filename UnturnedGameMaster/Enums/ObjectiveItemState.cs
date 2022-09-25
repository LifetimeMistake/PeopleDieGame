using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Enums
{
    public enum ObjectiveState 
    {
        AwaitingDrop, // Default idle state for objective items
        Roaming, // Set when item physically exists in the world
        Stored, // Set when item is placed inside acceptable player-made storage
        Secured // Set when item is placed inside of The Altar
    }
}
