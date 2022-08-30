using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class RespawnManager : IDisposableManager
    {
        [InjectDependency]
        private DataManager dataManager{ get; set; }
        [InjectDependency]
        private LoadoutManager loadoutManager{ get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager{ get; set; }
        [InjectDependency]
        private TeamManager teamManager{ get; set; }
        [InjectDependency]
        private GameManager gameManager{ get; set; }

        public event EventHandler<PlayerEventArgs> OnRespawnFinished;

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
            UnturnedChat.Say(player, "Witaj w świecie żywych!");
            RespawnPoint? worldRespawn = dataManager.GameData.DefaultRespawnPoint;

            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            if (playerData == null)
            {
                UnturnedChat.Say(player, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
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
