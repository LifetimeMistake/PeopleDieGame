using Rocket.Unturned.Events;
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
using UnturnedGameMaster.Helpers;
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
        
        public Dictionary<ushort, CachedItem> CachedItems { get; set; }

        [InjectDependency]
        private ArenaManager arenaManager { get; set; }

        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemAdded;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemRemoved;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemUpdated;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemSpawned;

        public void Init()
        {
            arenaManager.OnBossFightCompleted += ArenaManager_OnBossFightCompleted;
            UnturnedPlayerEvents.OnPlayerInventoryAdded += UnturnedPlayerEvents_OnPlayerInventoryAdded;
            UnturnedPlayerEvents.OnPlayerInventoryRemoved += UnturnedPlayerEvents_OnPlayerInventoryRemoved;
            CachedItems = new Dictionary<ushort, CachedItem>();
        }
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

        public void Dispose()
        {
            arenaManager.OnBossFightCompleted -= ArenaManager_OnBossFightCompleted;
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= UnturnedPlayerEvents_OnPlayerInventoryAdded;
            UnturnedPlayerEvents.OnPlayerInventoryRemoved -= UnturnedPlayerEvents_OnPlayerInventoryRemoved;
        }

        public bool CreateObjectiveItem(ushort itemId, BossArena arena, ObjectiveState objectiveState = ObjectiveState.AwaitingDrop)
        {
            ItemAsset item = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;
            if (item == null)
                return false;

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            if (objectiveItems.ContainsKey(itemId))
                return false;

            if (objectiveItems.FirstOrDefault(x => x.Value.ArenaId == arena.Id).Value != null)
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

        public bool MapObjectiveItemToArena(ushort itemId, BossArena arena)
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            if (!objectiveItems.ContainsKey(itemId))
                return false;

            ObjectiveItem objectiveItem = objectiveItems[itemId];
            objectiveItem.ArenaId = arena.Id;
            ObjectiveItemUpdated?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
            return true;
        }

        public Vector3S? GetObjectiveItemLocation(ushort itemId)
        {
            if (!CachedItems.ContainsKey(itemId))
                return null;

            CachedItem cachedItem = CachedItems[itemId];
            switch (cachedItem.GetLocation())
            {
                case CachedItemState.Ground:
                    return cachedItem.RegionItem.ItemData.point;
                case CachedItemState.Player:
                    return cachedItem.Player.Position;
                case CachedItemState.Vehicle:
                    return cachedItem.Vehicle.transform.position;
                case CachedItemState.Storage:
                    return cachedItem.Storage.transform.position;
                default:
                    return null;
            }
        }

        public ObjectiveItem[] GetObjectiveItems()
        {
            return dataManager.GameData.ObjectiveItems.Values.ToArray();
        }

        public ObjectiveItem GetObjectiveItem(ushort itemId)
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            if (!objectiveItems.ContainsKey(itemId))
                return null;

            return objectiveItems[itemId];
        }

        public bool ResetObjectiveItemState(ushort itemId)
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            if (!objectiveItems.ContainsKey(itemId))
                return false;

            ObjectiveItem objectiveItem = objectiveItems[itemId];
            objectiveItem.State = ObjectiveState.AwaitingDrop;
            ObjectiveItemUpdated?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
            return true;
        }

        private void ArenaManager_OnBossFightCompleted(object sender, BossFightEventArgs e)
        {
            BossArena arena = e.BossFight.Arena;

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            ObjectiveItem _objectiveItem = objectiveItems.Where(x => x.Value.ArenaId == arena.Id).FirstOrDefault().Value;

            ObjectiveItem objectiveItem = dataManager.GameData.ObjectiveItems[_objectiveItem.ItemId];

            if (objectiveItem == null)
                return;

            if (objectiveItem.State != ObjectiveState.AwaitingDrop)
                return;

            Item item = new Item(objectiveItem.ItemId, true);
            ItemManager.dropItem(item, arena.RewardSpawnPoint, true, true, false);

            CachedItem cachedItem = new CachedItem(objectiveItem.ItemId);
            cachedItem.RebuildCache();
            CachedItems.Add(cachedItem.Id, cachedItem);

            objectiveItem.State = ObjectiveState.Roaming;
            ObjectiveItemSpawned?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
        }

        private void UnturnedPlayerEvents_OnPlayerInventoryAdded(UnturnedPlayer player, Rocket.Unturned.Enumerations.InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            ObjectiveItem _objectiveItem = objectiveItems.Where(x => x.Value.ItemId == P.item.id).FirstOrDefault().Value;

            ObjectiveItem objectiveItem = dataManager.GameData.ObjectiveItems[_objectiveItem.ItemId];

            if (objectiveItem == null)
                return;

            CachedItem cachedItem = CachedItems[objectiveItem.ItemId];
            cachedItem.Player = player;
            cachedItem.State = CachedItemState.Player;
            objectiveItem.State = ObjectiveState.Stored;
        }

        private void UnturnedPlayerEvents_OnPlayerInventoryRemoved(UnturnedPlayer player, Rocket.Unturned.Enumerations.InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            ObjectiveItem _objectiveItem = objectiveItems.Where(x => x.Value.ItemId == P.item.id).FirstOrDefault().Value;

            ObjectiveItem objectiveItem = dataManager.GameData.ObjectiveItems[_objectiveItem.ItemId];

            if (objectiveItem == null)
                return;

            CachedItem cachedItem = CachedItems[objectiveItem.ItemId];
            CachedItemState locationState = cachedItem.GetLocation();

            switch (locationState)
            {
                
                case CachedItemState.Vehicle:
                case CachedItemState.Storage:
                    objectiveItem.State = ObjectiveState.Stored;
                    break;
                case CachedItemState.Ground:
                    objectiveItem.State = ObjectiveState.Roaming;
                    break;
                //default:
                //  whar

            }
        }
    }
}
