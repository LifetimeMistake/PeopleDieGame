using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
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

        private static Altar altar { get; set; }

        public event EventHandler<AltarSubmitEventArgs> OnAltarSubmitItems;

        public void Init()
        {
            ResizeReceptacles();
            if (gameManager.GetGameState() == GameState.InGame)
                RegisterTimers();
        }

        public void Dispose()
        {
            UnregisterTimers();
        }

        private void RegisterTimers()
        {
            UnregisterTimers();
            timerManager.Register(CheckAltarAbandoned, 300);
        }

        private void UnregisterTimers()
        {
            timerManager.Unregister(CheckAltarAbandoned);
        }

        private void ResizeReceptacles()
        {
            if (altar.Receptacles.Count == 0)
                return;

            foreach (InteractableStorage storage in altar.Receptacles)
            {
                storage.items.resize(2, 2);
            }
        }

        public Altar GetAltar()
        {
            if (altar == null)
                altar = dataManager.GameData.Altar;
            return altar;
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
            altar.Receptacles.Add(storage);
        }

        public bool ResetReceptacles()
        {
            if (altar.Receptacles.Count == 0)
                return false;

            altar.Receptacles = new List<InteractableStorage>();
            return true;
        }

        public bool IsPlayerInRadius(PlayerData playerData)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID((CSteamID)playerData.Id);

            return Vector3.Distance(altar.Position.Value, player.Position) <= altar.Radius;
        }

        public bool SubmitItems(PlayerData playerData)
        {
            if (!playerData.TeamId.HasValue)
                return false;

            Team team = teamManager.GetTeam(playerData.TeamId.Value);

            if (altar.Receptacles.Count == 0)
                throw new Exception("Altar does not have any receptacles");

            ObjectiveItem[] objectiveItems = objectiveManager.GetObjectiveItems();

            foreach (var storage in altar.Receptacles)
            {
                ItemJar item = storage.items.items.FirstOrDefault();
                if (item != null)
                {
                    if (objectiveItems.Any(x => x.ItemId == item.item.id))
                        continue;
                }
                return false;
            }

            OnAltarSubmitItems?.Invoke(this, new AltarSubmitEventArgs(team));
            return true;
        }

        private void CheckAltarAbandoned()
        {
            if (altar.Receptacles.Count == 0)
                return;

            List<InteractableStorage> fullStorageList = altar.Receptacles.Where(x => x.items.items.Count > 0).ToList();

            if (fullStorageList.Count == 0)
                return;

            ObjectiveItem[] objectiveItems = objectiveManager.GetObjectiveItems();

            List<InteractableStorage> storageList = fullStorageList.Where(x => objectiveItems.Any(y => y.ItemId == x.items.items.FirstOrDefault().item.id)).ToList();

            foreach (SteamPlayer player in Provider.clients)
            {
                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.playerID.steamID);
                if (!IsPlayerInRadius(playerData))
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

            ChatHelper.Say("Altar został pozostawiony z artefaktami w swoich pojemnikach. Wyczyszczono pojemniki i zrespiono artefakty.");
        }
    }
}
