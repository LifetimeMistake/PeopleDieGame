using Rocket.Unturned.Events;
using System;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;

namespace UnturnedGameMaster.Services.Managers
{
    public class RespawnManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private LoadoutManager loadoutManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private GameManager gameManager { get; set; }

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
            ChatHelper.Say(player, "Witaj w świecie żywych!");
            VectorPAR? worldRespawn = dataManager.GameData.DefaultRespawnPoint;

            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            if (playerData == null)
            {
                ChatHelper.Say(player, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
            }

            if ((playerData.TeamId == null || gameManager.GetGameState() == GameState.InLobby) && worldRespawn != null)
            {
                player.Teleport(worldRespawn.Value.Position, worldRespawn.Value.Rotation);
                ChatHelper.Say(player, "Budzisz się w globalnym punkcie zbiórki.");
            }
            else if (playerData.TeamId != null && (gameManager.GetGameState() == GameState.Intermission || gameManager.GetGameState() == GameState.InGame))
            {
                Team playerTeam = teamManager.GetTeam(playerData.TeamId.Value);
                VectorPAR? teamRespawn = playerTeam.RespawnPoint;

                if (teamRespawn != null)
                {
                    player.Teleport(teamRespawn.Value.Position, teamRespawn.Value.Rotation);
                    ChatHelper.Say(player, "Budzisz się w punkcie zbiórki twojej drużyny.");
                }

                if (playerTeam.DefaultLoadoutId != null)
                {
                    Loadout loadout = loadoutManager.GetLoadout(playerTeam.DefaultLoadoutId.Value);
                    if (loadout != null)
                    {
                        try
                        {
                            loadoutManager.GiveLoadout(playerData, loadout);
                        }
                        catch (Exception ex)
                        {
                            ExceptionHelper.Handle(ex, player, "Nie udało się nadać Tobie zestawu wyposażenia drużyny, skontaktuj się z administratorem.");
                        }
                    }
                }
            }
            else
            {
                ChatHelper.Say(player, "Budzisz się w szczerym polu.");
            }

            OnRespawnFinished?.Invoke(this, new PlayerEventArgs(playerData));
        }

        public void SetWorldRespawnPoint(VectorPAR? respawnPoint)
        {
            dataManager.GameData.DefaultRespawnPoint = respawnPoint;
        }
    }
}
