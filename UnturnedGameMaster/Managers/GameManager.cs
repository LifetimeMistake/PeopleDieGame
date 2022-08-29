using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Managers
{
    public class GameManager : IManager
    {
        private DataManager dataManager;
        public event EventHandler OnGameStateChanged;

        public GameManager(DataManager dataManager)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
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
    }
}
