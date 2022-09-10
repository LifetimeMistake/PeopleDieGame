using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using System;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;

namespace UnturnedGameMaster.Managers
{
    public class RewardManager : IDisposableManager
    {
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private ArenaManager arenaManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private DataManager dataManager { get; set; }
        

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
                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
                playerDataManager.DepositIntoWallet(playerData, arena.CompletionReward);
                playerDataManager.AddBounty(playerData, arena.CompletionBounty);
            }
        }

        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            PlayerData victimData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            PlayerData killerData = playerDataManager.GetPlayer((ulong)murderer);

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
            if (stat != SDG.Unturned.EPlayerStat.KILLS_ZOMBIES_NORMAL)
                return;

            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);

            if (playerData == null)
                return;

            ZombieKill(playerData);
        }

        public void PlayerKill(PlayerData victim, PlayerData killer)
        {
            double reward = dataManager.GameData.PlayerKillReward;
            double bounty = dataManager.GameData.Bounty;
            double total = reward + victim.Bounty;

            playerDataManager.AddBounty(killer, bounty);
            playerDataManager.DepositIntoWallet(killer, total);

            playerDataManager.ResetBounty(victim);
            playerDataManager.SetPlayerBalance(victim, Math.Round(victim.WalletBalance / 2));
            OnPlayerReceivePlayerReward?.Invoke(this, new RewardEventArgs(killer, total));
            OnPlayerReceiveDeathPenalty?.Invoke(this, new PlayerEventArgs(victim));
        }

        public void ZombieKill(PlayerData player)
        {
            double reward;
            reward = dataManager.GameData.ZombieKillReward;

            playerDataManager.DepositIntoWallet(player, reward);
            OnPlayerReceiveZombieReward?.Invoke(this, new RewardEventArgs(player, reward));
        }

        public void RandomDeath(PlayerData player)
        {
            playerDataManager.SetPlayerBalance(player, Math.Round(player.WalletBalance / 2));
            OnPlayerReceiveDeathPenalty?.Invoke(this, new PlayerEventArgs(player));
        }
    }
}
