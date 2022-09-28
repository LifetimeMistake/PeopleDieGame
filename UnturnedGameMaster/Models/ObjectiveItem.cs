using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class ObjectiveItem
    {
        public ushort ItemId;
        public ObjectiveState State;
        public int ArenaId;

        public ObjectiveItem(ushort itemId, ObjectiveState state, int arenaId)
        {
            ItemId = itemId;
            State = state;
            ArenaId = arenaId;
        }
    }
}
