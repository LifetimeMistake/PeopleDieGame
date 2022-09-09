using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;

namespace UnturnedGameMaster.Managers.EventMessageManagers
{
    public class RewardEventMessageManager : IManager
    {
        [InjectDependency]
        private RewardManager rewardManager { get; set; }

        public void Init()
        {
            rewardManager.OnPlayerReceiveDeathPenalty += RewardManager_OnPlayerReceiveDeathPenalty;
            rewardManager.OnPlayerReceivePlayerReward += RewardManager_OnPlayerReceivePlayerReward;
            rewardManager.OnPlayerReceiveZombieReward += RewardManager_OnPlayerReceiveZombieReward;
        }

        private void RewardManager_OnPlayerReceiveZombieReward(object sender, Models.EventArgs.RewardEventArgs e)
        {
            ChatHelper.Say(e.Player, $"Zabiłeś zombie i otrzymałeś ${e.Reward}");
        }

        private void RewardManager_OnPlayerReceivePlayerReward(object sender, Models.EventArgs.RewardEventArgs e)
        {
            ChatHelper.Say(e.Player, $"Zabiłeś gracza i otrzymałeś ${e.Reward}");
        }

        private void RewardManager_OnPlayerReceiveDeathPenalty(object sender, Models.EventArgs.PlayerEventArgs e)
        {
            ChatHelper.Say(e.Player, "Umarłeś i straciłeś 50% środków z portfela, lmao");
        }
    }
}
