using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class AltarManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private ObjectiveManager objectiveManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private GameManager gameManager { get; set; }
        [InjectDependency]
        private TimerManager timerManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private ArenaManager arenaManager { get; set; }

        private Altar altar { get => GetAltar(); }

        private List<InteractableStorage> receptacles = new List<InteractableStorage>();

        public event EventHandler<AltarSubmitEventArgs> OnAltarSubmitItems;

        public void Init()
        {
            
            objectiveManager.ObjectiveItemUpdated += ObjectiveManager_ObjectiveItemUpdated;
            gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
            if (gameManager.GetGameState() == GameState.InGame)
                RegisterTimers();
        }


        public void Dispose()
        {
            UnregisterTimers();
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

        private void RegisterTimers()
        {
            UnregisterTimers();
            timerManager.Register(CheckAltarAbandoned, 300);
            timerManager.Register(TryAddReceptacles, 60);
        }

        private void UnregisterTimers()
        {
            timerManager.Unregister(CheckAltarAbandoned);
        }

        private void TryAddReceptacles()
        {
            if (BarricadeManager.regions == null)
            {
                Debug.LogWarning("Attempted to load altar receptacles but in-game BarricadeManager was not ready yet (this message is harmless)");
                return;
            }

            foreach (InteractableStorage storage in UnityEngine.Object.FindObjectsOfType<InteractableStorage>())
            {
                if (IsPointInAltar(storage.transform.position))
                {
                    receptacles.Add(storage);
                    storage.items.resize(2, 2);
                }
            }
            Debug.Log($"Loaded {receptacles.Count} altar receptacles!");
            timerManager.Unregister(TryAddReceptacles);
        }

        private void ObjectiveManager_ObjectiveItemUpdated(object sender, ObjectiveItemEventArgs e)
        {
            if (e.ObjectiveItem.State == ObjectiveState.Stored)
            {
                CachedItem cachedItem = objectiveManager.GetItemCache(e.ObjectiveItem.ItemId);
                if (receptacles.Contains(cachedItem.Storage))
                {
                    e.ObjectiveItem.State = ObjectiveState.Secured;

                    ObjectiveItem[] securedItems = objectiveManager.GetObjectiveItems().Where(x => x.State == ObjectiveState.Secured).ToArray();
                    if (securedItems.Length != objectiveManager.GetObjectiveItems().Length)
                        return;
                    // now we know that all items are secured

                    PlayerData playerData = playerDataManager.GetData(cachedItem.LastOwnerId);

                    if (!playerData.TeamId.HasValue)
                    {
                        cachedItem.Storage.items.removeItem(0);

                        BossArena arena = arenaManager.GetArena(e.ObjectiveItem.ArenaId);
                        Vector3 ejectPosition = cachedItem.Storage.transform.position;
                        ejectPosition.z += 2;

                        objectiveManager.SpawnObjectiveItem(e.ObjectiveItem, ejectPosition);
                        ChatHelper.Say(playerData, "Artefakt wypadł z pojemnika (nie jesteś członkiem żadnej z drużyn!)");
                    }

                    Team team = teamManager.GetTeam(playerData.TeamId.Value);
                    altar.ItemsSubmitted = true;
                    OnAltarSubmitItems?.Invoke(this, new AltarSubmitEventArgs(team));
                }
                    
            }
        }

        public Altar GetAltar()
        {
            return dataManager.GameData.Altar;
        }

        public void SetAltarPosition(Vector3S position)
        {
            altar.SetPosition(position);
        }

        public void SetAltarRadius(double radius)
        {
            altar.SetRadius(radius);
        }

        public void AddReceptacle(InteractableStorage storage)
        {
            if (!UnityEngine.Object.FindObjectsOfType(typeof(InteractableStorage)).Contains(storage))
            {
                throw new ArgumentException("Storage cannot be found or does not exist");
            }

            storage.items.resize(2, 2);
            receptacles.Add(storage);
        }

        public bool ResetReceptacles()
        {
            if (receptacles.Count == 0)
                return false;

            receptacles.Clear();
            return true;
        }

        public bool IsPointInAltar(Vector3S point)
        {
            return Vector3.Distance(altar.Position.Value, point) <= altar.Radius;
        }

        public InteractableStorage[] GetReceptacles()
        {
            return receptacles.ToArray();
        }

        private void CheckAltarAbandoned()
        {
            if (Provider.clients.Count == 0)
                return;

            if (altar.ItemsSubmitted)
                return;

            if (receptacles.Count == 0)
                return;

            List<InteractableStorage> fullStorageList = receptacles.Where(x => x.items.items.Count > 0).ToList();

            if (fullStorageList.Count == 0)
                return;

            ObjectiveItem[] objectiveItems = objectiveManager.GetObjectiveItems();

            if (objectiveItems.Count() == 0)
                return;

            List<InteractableStorage> storageList = fullStorageList.Where(x => objectiveItems.Any(y => y.ItemId == x.items.items.FirstOrDefault().item.id)).ToList();

            foreach (SteamPlayer player in Provider.clients)
            {
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(player);
                
                if (!IsPointInAltar(unturnedPlayer.Position))
                    continue;
                return;
            }

            foreach (InteractableStorage storage in storageList)
            {
                ushort itemId = storage.items.items.FirstOrDefault().item.id;
                storage.items.removeItem(0);

                ObjectiveItem item = objectiveManager.GetObjectiveItem(itemId);
                if (item == null)
                    continue;

                BossArena arena = arenaManager.GetArena(item.ArenaId);
                objectiveManager.SpawnObjectiveItem(item, arena.RewardSpawnPoint);
            }

            ChatHelper.Say("Artefakty w altarze odleciały do domu :(");
        }
    }
}
