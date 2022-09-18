using Rocket.Unturned.Chat;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Services.Providers
{
    public class ArenaEventMessageProvider : IDisposableService
    {
        [InjectDependency]
        private ArenaManager arenaManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }

        public void Init()
        {
            arenaManager.OnBossFightRemoved += ArenaManager_OnBossFightRemoved;
            arenaManager.OnBossFightCreated += ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightDominantTeamChanged += ArenaManager_OnBossFightDominantTeamChanged;
        }

        public void Dispose()
        {
            arenaManager.OnBossFightRemoved -= ArenaManager_OnBossFightRemoved;
            arenaManager.OnBossFightCreated -= ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightDominantTeamChanged -= ArenaManager_OnBossFightDominantTeamChanged;
        }

        private void ArenaManager_OnBossFightDominantTeamChanged(object sender, Models.EventArgs.BossFightDominationEventArgs e)
        {
            Team dominantTeam = e.NewAttackers;
            Team oldTeam = e.OldAttackers;

            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(dominantTeam))
            {
                ChatHelper.Say(player, $"Twoja drużyna uzyskała status dominującej w arenie");
            }

            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(oldTeam))
            {
                ChatHelper.Say(player, $"Twoja drużyna straciła status dominującej w arenie");
            }
        }

        private void ArenaManager_OnBossFightRemoved(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            Team team = e.BossFight.DominantTeam;
            BossArena arena = e.BossFight.Arena;

            switch (e.BossFight.State)
            {
                case Enums.BossFightState.BossDefeated:
                    Color red = UnturnedChat.GetColorFromRGB(255, 0, 0);
                    // used UnturnedChat to give the message a color, because kil boss cool B)
                    UnturnedChat.Say($"Drużyna \"{team.Name}\" pokonała boss'a \"{arena.BossModel.Name}\" i otrzymała jego klucz!", red);
                    break;
                case Enums.BossFightState.AttackersDefeated:
                    ChatHelper.Say($"Drużyna \"{team.Name}\" została pokonana przez \"{arena.BossModel.Name}\"!");
                    break;
                case Enums.BossFightState.Abandoned:
                    ChatHelper.Say($"Drużyna \"{team.Name}\" przestraszyła się \"{arena.BossModel.Name}\" i uciekła z walki!");
                    break;
                default:
                    ChatHelper.Say($"Walka z bossem \"{arena.BossModel.Name}\" została zakończona z powodu błedu serwera. sus");
                    break;
            }
        }

        private void ArenaManager_OnBossFightCreated(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            Team team = e.BossFight.DominantTeam;
            BossArena arena = e.BossFight.Arena;

            ChatHelper.Say($"Drużyna \"{team.Name}\" rozpoczęła walkę z boss'em \"{arena.BossModel.Name}\"");
        }
    }
}
