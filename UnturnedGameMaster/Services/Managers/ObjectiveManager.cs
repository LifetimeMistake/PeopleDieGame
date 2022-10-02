using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [InjectDependency]
        private TimerManager timerManager { get; set; }
        [InjectDependency]
        private GameManager gameManager { get; set; }

        private Dictionary<ushort, CachedItem> cachedItems = new Dictionary<ushort, CachedItem>();
        private Dictionary<ushort, int> SearchCount { get; set; }

        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemAdded;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemRemoved;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemUpdated;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemSpawned;

        public void Init()
        {
            arenaManager.OnArenaRemoved += ArenaManager_OnArenaRemoved;
            arenaManager.OnBossFightCompleted += ArenaManager_OnBossFightCompleted;
            UnturnedEvents.Instance.OnPlayerDisconnected += Instance_OnPlayerDisconnected;
            RespawnObjecitveItems();
            gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
            if (gameManager.GetGameState() == GameState.InGame)
                RegisterTimers();
        }

        public void Dispose()
        {
            SetLastLocations();
            arenaManager.OnArenaRemoved -= ArenaManager_OnArenaRemoved;
            arenaManager.OnBossFightCompleted -= ArenaManager_OnBossFightCompleted;
            UnturnedEvents.Instance.OnPlayerDisconnected -= Instance_OnPlayerDisconnected;
            gameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
            UnregisterTimers();
        }

        private void RegisterTimers()
        {
            UnregisterTimers();
            timerManager.Register(ValidateCaches, 60);
        }

        private void UnregisterTimers()
        {
            timerManager.Unregister(ValidateCaches);
        }

        private void RespawnObjecitveItems()
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            Dictionary<ushort, Vector3S?> lastLocations = dataManager.GameData.LastObjectiveItemLocations;

            foreach (KeyValuePair<ushort, ObjectiveItem> kvp in objectiveItems)
            {
                ObjectiveItem objectiveItem = kvp.Value;   
                if (kvp.Value.State == ObjectiveState.Roaming)
                {
                    Item item = new Item(objectiveItem.ItemId, true);
                    ItemManager.dropItem(item, (Vector3S)lastLocations[objectiveItem.ItemId], true, true, false);

                    CachedItem cachedItem = new CachedItem(objectiveItem.ItemId);
                    cachedItem.RebuildCache();
                    cachedItems.Add(cachedItem.Id, cachedItem);

                    ObjectiveItemSpawned?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
                }
            }
        }

        private void SetLastLocations()
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            Dictionary<ushort, Vector3S?> lastLocations = dataManager.GameData.LastObjectiveItemLocations;

            foreach (KeyValuePair<ushort, ObjectiveItem> kvp in objectiveItems)
            {
                Vector3S? location = GetObjectiveItemLocation(kvp.Key);

                if (location != null)
                {
                    if (lastLocations.ContainsKey(kvp.Key))
                    {
                        lastLocations[kvp.Key] = location;
                    }
                    else
                    {
                        lastLocations.Add(kvp.Key, location);
                    }
                }

            }
        }

        private void GameManager_OnGameStateChanged(object sender, EventArgs e)
        {
            GameState gameState = gameManager.GetGameState();
            if (gameState == GameState.InGame)
            {
                RegisterTimers();
            }
            else
            {
                UnregisterTimers();
            }
        }

        private void ValidateCaches()
        {
            if (cachedItems.Count == 0)
                return;

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;

            foreach (KeyValuePair<ushort, CachedItem> kvp in cachedItems)
            {
                CachedItem item = kvp.Value;
                if (!item.ValidateCache())
                    item.RebuildCache();


                ObjectiveItem objectiveItem = objectiveItems[item.Id];

                switch (item.State)
                {
                    case CachedItemState.Ground:
                    case CachedItemState.Player:
                    case CachedItemState.Vehicle:
                        objectiveItem.State = ObjectiveState.Roaming;
                        break;
                    case CachedItemState.Storage:
                        objectiveItem.State = ObjectiveState.Stored;
                        break;
                    default:
                        objectiveItem.State = ObjectiveState.Lost;
                        SearchLostItems(objectiveItem);
                        break;
                }
            }
        }

        private void SearchLostItems(ObjectiveItem objectiveItem)
        {
            CachedItem cachedItem = cachedItems[objectiveItem.ItemId];

            CachedItemState state = cachedItem.GetLocation();
            if (state == CachedItemState.Unknown)
                SearchCount[cachedItem.Id]++;
            else
                SearchCount[cachedItem.Id] = 0;

            if (SearchCount[cachedItem.Id] == 3)
            {
                BossArena arena = arenaManager.GetArena(objectiveItem.ArenaId);

                SearchCount[cachedItem.Id] = 0;

                Item item = new Item(objectiveItem.ItemId, true);
                ItemManager.dropItem(item, arena.RewardSpawnPoint, true, true, false);
                cachedItem.RebuildCache();

                objectiveItem.State = ObjectiveState.Roaming;
                ObjectiveItemSpawned?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));

                ChatHelper.Say($"Próba znalezienia artefaktu areny {arena.Name} zakończyła się niepowodzeniem, artefakt został zrespiony w odpowiedniej arenie");
            }
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
            if (!cachedItems.ContainsKey(itemId))
                return null;

            CachedItem cachedItem = cachedItems[itemId];
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

        public CachedItem[] GetCachedItems()
        {
            return cachedItems.Values.ToArray();
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
            KeyValuePair<ushort, ObjectiveItem> query = objectiveItems.Where(x => x.Value.ArenaId == arena.Id).FirstOrDefault();
            ObjectiveItem objectiveItem = query.Equals(null) ? null : query.Value;

            if (objectiveItem == null)
                return;

            if (objectiveItem.State != ObjectiveState.AwaitingDrop)
                return;

            Item item = new Item(objectiveItem.ItemId, true);
            ItemManager.dropItem(item, arena.RewardSpawnPoint, true, true, false);

            CachedItem cachedItem = new CachedItem(objectiveItem.ItemId);
            cachedItem.RebuildCache();
            cachedItems.Add(cachedItem.Id, cachedItem);

            SearchCount.Add(cachedItem.Id, 0);

            objectiveItem.State = ObjectiveState.Roaming;
            ObjectiveItemSpawned?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
        }

        private void ArenaManager_OnArenaRemoved(object sender, ArenaEventArgs e)
        {
            // Remove all connected objective items

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            foreach (ObjectiveItem objectiveItem in objectiveItems.Values.Where(x => x.ArenaId == e.Arena.Id))
                objectiveItems.Remove(objectiveItem.ItemId);
        }

        private void Instance_OnPlayerDisconnected(UnturnedPlayer player)
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            foreach (ObjectiveItem objectiveItem in objectiveItems.Values)
            {
                InventorySearch search = player.Inventory.has(objectiveItem.ItemId);
                if (search != null)
                {
                    byte index = player.Inventory.items[search.page].getIndex(search.jar.x, search.jar.y);
                    player.Inventory.removeItem(search.page, index);
                    player.Inventory.save();

                    Item item = new Item(objectiveItem.ItemId, true);
                    ItemManager.dropItem(item, player.Position, true, true, false);
                    ObjectiveItemSpawned?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
                }
            }
        }
    }
}
