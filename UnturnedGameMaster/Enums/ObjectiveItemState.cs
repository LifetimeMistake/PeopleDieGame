using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Enums
{
    public enum ObjectiveState 
    {
        /// <summary>
        /// Default idle state for objective items
        /// </summary>
        AwaitingDrop,
        /// <summary>
        /// Set when item physically exists in the world
        /// </summary>
        Roaming,
        /// <summary>
        /// Set when item is placed inside acceptable player-made storage
        /// </summary>
        Stored,
        /// <summary>
        /// Set when item is placed inside of The Altar
        /// </summary>
        Secured,
        Lost
    }
}
