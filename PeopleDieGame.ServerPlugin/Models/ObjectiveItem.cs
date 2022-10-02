using PeopleDieGame.ServerPlugin.Enums;

namespace PeopleDieGame.ServerPlugin.Models
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
