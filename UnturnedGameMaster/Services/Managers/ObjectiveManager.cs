using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;

namespace UnturnedGameMaster.Services.Managers
{
    internal class ObjectiveManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private ArenaManager arenaManager { get; set; }

        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemAdded;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemRemoved;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemUpdated;

        public void Init()
        {
            arenaManager.OnArenaRemoved += ArenaManager_OnArenaRemoved;
        }

        public void Dispose()
        {
            arenaManager.OnArenaRemoved -= ArenaManager_OnArenaRemoved;
        }

        private void ArenaManager_OnArenaRemoved(object sender, ArenaEventArgs e)
        {
            // Remove all connected objective items

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            foreach (ObjectiveItem objectiveItem in objectiveItems.Values.Where(x => x.ArenaId == e.Arena.Id))
                objectiveItems.Remove(objectiveItem.ItemId);
        }

        public bool CreateObjectiveItem(ushort itemId, BossArena arena, ObjectiveState objectiveState = ObjectiveState.AwaitingDrop)
        {
            ItemAsset item = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;
            if (item == null)
                return false;

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            if (objectiveItems.ContainsKey(itemId))
                return false;

            ObjectiveItem objectiveItem = new ObjectiveItem(itemId, objectiveState, arena.Id);
            objectiveItems.Add(itemId, objectiveItem);
            ObjectiveItemAdded?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
            return true;
        }

        public bool RemoveObjectiveItem(ushort itemId)
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            if (!objectiveItems.ContainsKey(itemId))
                return false;

            ObjectiveItem objectiveItem = objectiveItems[itemId];
            objectiveItems.Remove(itemId);
            ObjectiveItemRemoved?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
            return true;
        }

        public ObjectiveItem[] GetObjectiveItems()
        {
            return dataManager.GameData.ObjectiveItems.Values.ToArray();
        }
    }
}
