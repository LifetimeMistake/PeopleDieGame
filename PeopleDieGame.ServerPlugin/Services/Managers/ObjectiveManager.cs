using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
using Pathfinding.RVO.Sampled;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Services.Managers
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
        private Dictionary<ushort, int> searchAttemptCount = new Dictionary<ushort, int>();

        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemAdded;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemRemoved;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemUpdated;
        public event EventHandler<ObjectiveItemEventArgs> ObjectiveItemSpawned;

        public void Init()
        {
            InitCache();
            arenaManager.OnArenaRemoved += ArenaManager_OnArenaRemoved;
            arenaManager.OnBossFightCompleted += ArenaManager_OnBossFightCompleted;
            UnturnedEvents.Instance.OnPlayerDisconnected += Instance_OnPlayerDisconnected;
            gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
            if (gameManager.GetGameState() == GameState.InGame)
                RegisterTimers();
        }

        public void Dispose()
        {
            arenaManager.OnArenaRemoved -= ArenaManager_OnArenaRemoved;
            arenaManager.OnBossFightCompleted -= ArenaManager_OnBossFightCompleted;
            UnturnedEvents.Instance.OnPlayerDisconnected -= Instance_OnPlayerDisconnected;
            gameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
            UnregisterTimers();
        }

        private void InitCache()
        {
            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            foreach (ObjectiveItem item in objectiveItems.Values)
            {
                if (item.State == ObjectiveState.AwaitingDrop)
                    continue;

                CachedItem cachedItem = new CachedItem(item.ItemId);
                cachedItems.Add(item.ItemId, cachedItem);
            }
        }

        private void RegisterTimers()
        {
            UnregisterTimers();
            timerManager.Register(ValidateCaches, 300);
        }

        private void UnregisterTimers()
        {
            timerManager.Unregister(ValidateCaches);
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

        private void SpawnObjectiveItem(ObjectiveItem objectiveItem, Vector3 spawnPoint)
        {
            Item item = new Item(objectiveItem.ItemId, true);
            ItemManager.dropItem(item, spawnPoint, true, true, false);

            if (cachedItems.ContainsKey(objectiveItem.ItemId))
                cachedItems[objectiveItem.ItemId].RebuildCache();

            objectiveItem.State = ObjectiveState.Roaming;
            ObjectiveItemSpawned?.Invoke(this, new ObjectiveItemEventArgs(objectiveItem));
        }

        private void ValidateCaches()
        {
            if (cachedItems.Count == 0)
                return;

            if (!Level.isLoaded)
                return;

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            foreach (CachedItem item in cachedItems.Values)
            {
                ObjectiveItem objectiveItem = objectiveItems[item.Id];
                CachedItemLocation location = item.GetLocation();

                switch (location)
                {
                    case CachedItemLocation.Ground:
                    case CachedItemLocation.Player:
                    case CachedItemLocation.Vehicle:
                        objectiveItem.State = ObjectiveState.Roaming;
                        break;
                    case CachedItemLocation.Storage:
                        objectiveItem.State = ObjectiveState.Stored;
                        break;
                    default:
                        objectiveItem.State = ObjectiveState.Lost;
                        if (searchAttemptCount.ContainsKey(item.Id))
                            searchAttemptCount[item.Id]++;
                        else
                            searchAttemptCount.Add(item.Id, 1);

                        if (searchAttemptCount[item.Id] == 5)
                        {
                            BossArena arena = arenaManager.GetArena(objectiveItem.ArenaId);
                            SpawnObjectiveItem(objectiveItem, arena.RewardSpawnPoint);
                            searchAttemptCount.Remove(item.Id);
                            ChatHelper.Say($"Jednemu z artefaktów wyrosły nogi i uciekł, złapaliśmy go i umieściliśmy na arenie \"{arena.Name}\"!");
                        }
                        break;
                }
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
            cachedItems.Remove(itemId);
            searchAttemptCount.Remove(itemId);

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

        public CachedItemLocation GetObjectiveItemLocation(ushort itemId)
        {
            if (!cachedItems.ContainsKey(itemId))
                return CachedItemLocation.Unknown;

            CachedItem cachedItem = cachedItems[itemId];
            return cachedItem.GetLocation();
        }

        public Vector3S? GetObjectiveItemPosition(ushort itemId)
        {
            if (!cachedItems.ContainsKey(itemId))
                return null;

            CachedItem cachedItem = cachedItems[itemId];
            switch (cachedItem.GetLocation())
            {
                case CachedItemLocation.Ground:
                    return cachedItem.RegionItem.ItemData.point;
                case CachedItemLocation.Player:
                    return cachedItem.Player.Position;
                case CachedItemLocation.Vehicle:
                    return cachedItem.Vehicle.transform.position;
                case CachedItemLocation.Storage:
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
            foreach(ObjectiveItem objectiveItem in objectiveItems.Where(x => x.Value.ArenaId == arena.Id).Select(x => x.Value))
            {
                if (objectiveItem.State != ObjectiveState.AwaitingDrop)
                    return;

                if (!cachedItems.ContainsKey(objectiveItem.ItemId))
                {
                    CachedItem cachedItem = new CachedItem(objectiveItem.ItemId);
                    cachedItems.Add(objectiveItem.ItemId, cachedItem);
                }
                
                SpawnObjectiveItem(objectiveItem, arena.RewardSpawnPoint);
            }
        }

        private void ArenaManager_OnArenaRemoved(object sender, ArenaEventArgs e)
        {
            // Remove all connected objective items

            Dictionary<ushort, ObjectiveItem> objectiveItems = dataManager.GameData.ObjectiveItems;
            foreach (ObjectiveItem objectiveItem in objectiveItems.Values.Where(x => x.ArenaId == e.Arena.Id))
                RemoveObjectiveItem(objectiveItem.ItemId);
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

                    SpawnObjectiveItem(objectiveItem, player.Position);
                }
            }
        }
    }
}
