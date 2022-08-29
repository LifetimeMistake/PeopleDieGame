using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class GameManager : IManager
    {
        [InjectDependency]
        private DataManager dataManager{ get; set; }
        [InjectDependency]
        private TeamManager teamManager{ get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager{ get; set; }

        public event EventHandler OnGameStateChanged;

        public void Init()
        { }

        public GameState GetGameState()
        {
            return dataManager.GameData.State;
        }

        public void SetGameState(GameState state)
        {
            dataManager.GameData.State = state;
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void StartGame()
        {

        }

        public void EndGame()
        {

        }

        public string GetGameSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Witaj jasiu na serverze hentai!");
            sb.AppendLine($"Obecnie mierzy się z sobą {playerDataManager.GetPlayerCount() - 1} innych graczy z {teamManager.GetTeamCount()} drużyn");
            sb.AppendLine($"Na serwerze online jest {Provider.clients.Count} osób");
            sb.AppendLine($"Obecny stan gry: {GameStateFriendlyNameProvider.GetFriendlyName(GetGameState())}");

            return sb.ToString();
        }
    }
}
