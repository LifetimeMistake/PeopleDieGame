using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using System;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class RewardManager : IDisposableService
    {
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private ArenaManager arenaManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private GameManager gameManager { get; set; }
        [InjectDependency]
        private LoadoutManager loadoutManager { get; set; }

        public event EventHandler<RewardEventArgs> OnPlayerReceivePlayerReward;
        public event EventHandler<RewardEventArgs> OnPlayerReceiveZombieReward;
        public event EventHandler<PlayerEventArgs> OnPlayerReceiveDeathPenalty;

        public void Init()
        {
            UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerUpdateStat += UnturnedPlayerEvents_OnPlayerUpdateStat;
            arenaManager.OnBossFightCompleted += ArenaManager_OnBossFightCompleted;
        }

        public void Dispose()
        {
            UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerUpdateStat -= UnturnedPlayerEvents_OnPlayerUpdateStat;
            arenaManager.OnBossFightCompleted -= ArenaManager_OnBossFightCompleted;
        }

        private void ArenaManager_OnBossFightCompleted(object sender, BossFightEventArgs e)
        {
            Team team = e.BossFight.DominantTeam;
            BossArena arena = e.BossFight.Arena;

            foreach (UnturnedPlayer player in arenaManager.GetPlayersInsideArena(arena, team))
            {
                PlayerData playerData = playerDataManager.GetData((ulong)player.CSteamID);
                playerDataManager.AddBalance(playerData, arena.CompletionReward);
                playerDataManager.AddBounty(playerData, arena.CompletionBounty);
            }

            if (!arena.RewardLoadoutId.HasValue)
                return;

            Loadout reward = loadoutManager.GetLoadout(arena.RewardLoadoutId.Value);
            if (reward == null)
                return;

            loadoutManager.DropLoadout(reward, arena.RewardSpawnPoint);
        }

        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            if (gameManager.GetGameState() == Enums.GameState.InLobby)
            {
                return;
            }

            PlayerData victimData = playerDataManager.GetData((ulong)player.CSteamID);
            PlayerData killerData = playerDataManager.GetData((ulong)murderer);

            if (victimData == null)
                return;

            if (killerData == null)
            {
                RandomDeath(victimData);
                return;
            }

            switch (cause)
            {
                case SDG.Unturned.EDeathCause.GUN:
                case SDG.Unturned.EDeathCause.MELEE:
                case SDG.Unturned.EDeathCause.PUNCH:
                case SDG.Unturned.EDeathCause.ROADKILL:
                    PlayerKill(victimData, killerData);
                    break;
                case SDG.Unturned.EDeathCause.GRENADE:
                    if (player.CSteamID == murderer)
                    {
                        RandomDeath(victimData);
                        break;
                    }
                    PlayerKill(victimData, killerData);
                    break;
                case SDG.Unturned.EDeathCause.MISSILE:
                    if (player.CSteamID == murderer)
                    {
                        RandomDeath(victimData);
                        break;
                    }
                    PlayerKill(victimData, killerData);
                    break;
                default:
                    RandomDeath(victimData);
                    break;
            }
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateStat(UnturnedPlayer player, SDG.Unturned.EPlayerStat stat)
        {
            if (gameManager.GetGameState() == Enums.GameState.InLobby)
            {
                return;
            }

            if (stat != SDG.Unturned.EPlayerStat.KILLS_ZOMBIES_NORMAL)
                return;

            PlayerData playerData = playerDataManager.GetData((ulong)player.CSteamID);

            if (playerData == null)
                return;

            ZombieKill(playerData);
        }

        private void PlayerKill(PlayerData victim, PlayerData killer)
        {
            float reward = dataManager.GameData.PlayerKillReward;
            float bounty = dataManager.GameData.Bounty;
            float total = reward + victim.Bounty;

            playerDataManager.AddBalance(killer, total);
            playerDataManager.AddBounty(killer, bounty);

            playerDataManager.UpdateBalance(victim, Mathf.Round(victim.WalletBalance / 2));
            playerDataManager.UpdateBounty(victim, 0);
            OnPlayerReceivePlayerReward?.Invoke(this, new RewardEventArgs(killer, total));
            OnPlayerReceiveDeathPenalty?.Invoke(this, new PlayerEventArgs(victim));
        }

        private void ZombieKill(PlayerData player)
        {
            float reward;
            reward = dataManager.GameData.ZombieKillReward;

            playerDataManager.AddBalance(player, reward);
            OnPlayerReceiveZombieReward?.Invoke(this, new RewardEventArgs(player, reward));
        }

        private void RandomDeath(PlayerData player)
        {
            playerDataManager.UpdateBalance(player, Mathf.Round(player.WalletBalance / 2));
            OnPlayerReceiveDeathPenalty?.Invoke(this, new PlayerEventArgs(player));
        }
    }
}
