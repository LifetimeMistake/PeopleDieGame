using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;

namespace UnturnedGameMaster.Managers
{
    public class RewardManager : IManager
    {
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private DataManager dataManager { get; set; }

        public event EventHandler<PlayerEventArgs> OnPlayerReceivePlayerReward;
        public event EventHandler<PlayerEventArgs> OnPlayerReceiveZombieReward;
        public event EventHandler<PlayerEventArgs> OnPlayerReceiveDeathPenalty;

        public void Init()
        {
            UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerUpdateStat += UnturnedPlayerEvents_OnPlayerUpdateStat;
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
                    PlayerKill(victimData, killerData);
                    break;
                case SDG.Unturned.EDeathCause.MELEE:
                    PlayerKill(victimData, killerData);
                    break;
                case SDG.Unturned.EDeathCause.PUNCH:
                    PlayerKill(victimData, killerData);
                    break;
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

            killer.AddBounty(bounty);
            killer.Deposit(reward + victim.Bounty);

            victim.ResetBounty();
            victim.SetBalance(Math.Round(victim.WalletBalance/2));
            OnPlayerReceivePlayerReward?.Invoke(this, new PlayerEventArgs(killer));
            OnPlayerReceiveDeathPenalty?.Invoke(this, new PlayerEventArgs(victim));
        }

        public void ZombieKill(PlayerData player)
        {
            double reward;
            reward = dataManager.GameData.ZombieKillReward;

            player.Deposit(reward);
            OnPlayerReceiveZombieReward?.Invoke(this, new PlayerEventArgs(player));
        }

        public void RandomDeath(PlayerData player)
        {
            player.SetBalance(Math.Round(player.WalletBalance / 2));
            OnPlayerReceiveDeathPenalty?.Invoke(this, new PlayerEventArgs(player));
        }
    }
}
