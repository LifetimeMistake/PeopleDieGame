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

            if (cause == SDG.Unturned.EDeathCause.KILL)
            {
                UnturnedChat.Say("player kill :)");
                //if (victimData == null || killerData == null)
                //    return;

                //PlayerKill(victimData, killerData);
            }
            else
            {
                UnturnedChat.Say("not player kill :(");
                //victimData.SetBalance(Math.Round(victimData.WalletBalance / 2));
            }
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateStat(UnturnedPlayer player, SDG.Unturned.EPlayerStat stat)
        {
            if (stat != SDG.Unturned.EPlayerStat.KILLS_ZOMBIES_NORMAL || stat != SDG.Unturned.EPlayerStat.KILLS_ZOMBIES_MEGA)
                return;

            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);

            if (playerData == null)
                return;

            bool isMega;

            if (stat == SDG.Unturned.EPlayerStat.KILLS_ZOMBIES_MEGA)
                isMega = true;
            else
                isMega = false;

            ZombieKill(playerData, isMega);
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

        public void ZombieKill(PlayerData player, bool isMega)
        {
            double reward;
            if (isMega)
                reward = dataManager.GameData.MegaZombieKillReward;
            else
                reward = dataManager.GameData.ZombieKillReward;

            player.Deposit(reward);
            OnPlayerReceiveZombieReward?.Invoke(this, new PlayerEventArgs(player));
        }
    }
}
