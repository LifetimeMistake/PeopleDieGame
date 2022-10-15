using PeopleDieGame.NetMethods.Managers;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class BossBarProvider : IDisposableService
    {
        [InjectDependency]
        private ArenaManager arenaManager { get; set; }
        [InjectDependency]
        private TimerManager timerManager { get; set; }

        public void Init()
        {
            arenaManager.OnPlayerJoinedFight += ArenaManager_OnPlayerJoinedFight;
            arenaManager.OnPlayerLeftFight += ArenaManager_OnPlayerLeftFight;
            arenaManager.OnBossFightCreated += ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightRemoved += ArenaManager_OnBossFightRemoved;
            timerManager.Register(UpdateBossBars, 30);
        }

        public void Dispose()
        {
            arenaManager.OnPlayerJoinedFight -= ArenaManager_OnPlayerJoinedFight;
            arenaManager.OnPlayerLeftFight -= ArenaManager_OnPlayerLeftFight;
            arenaManager.OnBossFightCreated -= ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightRemoved -= ArenaManager_OnBossFightRemoved;
            timerManager.Unregister(UpdateBossBars);
        }

        private void ArenaManager_OnPlayerJoinedFight(object sender, Models.EventArgs.BossFightParticipantEventArgs e)
        {
            IZombieModel bossModel = e.BossFight.FightController.GetBossBase();
            float health = (float)e.BossFight.FightController.GetBossHealthPercentage();
            BossBarManager.UpdateBossBar(bossModel.Name, health, e.Player.SteamPlayer());
        }

        private void ArenaManager_OnPlayerLeftFight(object sender, Models.EventArgs.BossFightParticipantEventArgs e)
        {
            BossBarManager.RemoveBossBar(e.Player.SteamPlayer());
        }

        private void ArenaManager_OnBossFightCreated(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            IZombieModel bossModel = e.BossFight.FightController.GetBossBase();
            float health = (float)e.BossFight.FightController.GetBossHealthPercentage();
            foreach (UnturnedPlayer player in e.BossFight.Participants)
                BossBarManager.UpdateBossBar(bossModel.Name, health, player.SteamPlayer());
        }

        private void ArenaManager_OnBossFightRemoved(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            foreach (UnturnedPlayer player in e.BossFight.Participants)
                BossBarManager.RemoveBossBar(player.SteamPlayer());
        }

        private void UpdateBossBars()
        {
            foreach (BossFight bossFight in arenaManager.GetBossFights().Where(x => x.State != BossFightState.Idle))
            {
                IZombieModel bossModel = bossFight.Arena.BossModel;

                foreach (UnturnedPlayer player in bossFight.Participants)
                {
                    string name = bossModel.Name;
                    float health = (float)bossFight.FightController.GetBossHealthPercentage();

                    BossBarManager.UpdateBossBar(name, health, player.SteamPlayer());
                }
            }
        }
    }
}
