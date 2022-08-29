using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class GameManager : IManager
    {
        private DataManager dataManager;
        private TeamManager teamManager;
        private GameManager gameManager;
        private PlayerDataManager playerDataManager;

        public event EventHandler OnGameStateChanged;

        public GameManager(DataManager dataManager, TeamManager teamManager, GameManager gameManager, PlayerDataManager playerDataManager)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            this.teamManager = teamManager ?? throw new ArgumentNullException(nameof(teamManager));
            this.gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            this.playerDataManager = playerDataManager ?? throw new ArgumentNullException(nameof(playerDataManager));
        }

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
            sb.AppendLine($"Obecny stan gry: {GameStateFriendlyNameProvider.GetFriendlyName(gameManager.GetGameState())}");

            return sb.ToString();
        }
    }
}
