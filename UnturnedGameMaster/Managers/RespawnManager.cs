using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class RespawnManager : IDisposableManager
    {
        private DataManager dataManager;
        private LoadoutManager loadoutManager;
        private PlayerDataManager playerDataManager;
        private TeamManager teamManager;
        private GameManager gameManager;

        public event EventHandler<PlayerEventArgs> OnRespawnFinished;

        public RespawnManager(DataManager dataManager, LoadoutManager loadoutManager, PlayerDataManager playerDataManager, TeamManager teamManager, GameManager gameManager)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            this.loadoutManager = loadoutManager ?? throw new ArgumentNullException(nameof(loadoutManager));
            this.playerDataManager = playerDataManager ?? throw new ArgumentNullException(nameof(playerDataManager));
            this.teamManager = teamManager ?? throw new ArgumentNullException(nameof(teamManager));
            this.gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
        }

        public void Init()
        {
            UnturnedPlayerEvents.OnPlayerRevive += UnturnedPlayerEvents_OnPlayerRevive;
        }

        public void Dispose()
        {
            UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
        }

        private void UnturnedPlayerEvents_OnPlayerRevive(Rocket.Unturned.Player.UnturnedPlayer player, UnityEngine.Vector3 position, byte angle)
        {
            PlayerEventArgs playerEventArgs = new PlayerEventArgs(player);
            UnturnedChat.Say("Witaj w świecie żywych!");
            RespawnPoint? worldRespawn = dataManager.GameData.DefaultRespawnPoint;

            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            if (playerData == null)
            {
                UnturnedChat.Say(player, "Wystąpił błąd (nie można odnaleźć akt gracza??)");
            }

            if((playerData.TeamId == null || gameManager.GetGameState() == GameState.InLobby) && worldRespawn != null)
            {
                player.Teleport(worldRespawn.Value.Position, worldRespawn.Value.Rotation);
                UnturnedChat.Say(player, "Budzisz się w globalnym punkcie zbiórki.");
            }
            else if (playerData.TeamId != null && (gameManager.GetGameState() == GameState.Intermission || gameManager.GetGameState() == GameState.InGame))
            {
                Team playerTeam = teamManager.GetTeam(playerData.TeamId.Value);
                RespawnPoint? teamRespawn = playerTeam.RespawnPoint;

                if (teamRespawn != null)
                {
                    player.Teleport(teamRespawn.Value.Position, teamRespawn.Value.Rotation);
                    UnturnedChat.Say(player, "Budzisz się w punkcie zbiórki twojej drużyny.");
                }

                if (playerTeam.DefaultLoadoutId != null)
                {
                    Loadout loadout = loadoutManager.GetLoadout(playerTeam.DefaultLoadoutId.Value);
                    if (loadout != null)
                    {
                        loadoutManager.GiveLoadout(player, loadout);
                    }
                }
            }
            else
            {
                UnturnedChat.Say(player, "Budzisz się w szczerym polu.");
            }

            OnRespawnFinished?.Invoke(this, playerEventArgs);
        }

        public void SetWorldRespawnPoint(RespawnPoint? respawnPoint)
        {
            dataManager.GameData.DefaultRespawnPoint = respawnPoint;
        }
    }
}
