using Rocket.Unturned.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class PlayerDataManager : IDisposableManager
    {
        private DataManager dataManager;
        
        public PlayerDataManager(DataManager dataManager)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
        }

        public void Init()
        {
            UnturnedEvents.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
        }

        public void Dispose()
        {
            UnturnedEvents.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
        }

        private void Instance_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            List<PlayerData> playerList = dataManager.GameData.PlayerData;
            if (playerList.Any(x => x.Id == (ulong)player.CSteamID))
                return; // no need to register a new player data struct, this player has been here before

            // register a new player
            PlayerData playerData = new PlayerData((ulong)player.CSteamID, "Bardzo tajemniczy...");
            playerList.Add(playerData);
        }

        public PlayerData GetPlayer(ulong id)
        {
            List<PlayerData> playerList = dataManager.GameData.PlayerData;
            return playerList.FirstOrDefault(x => x.Id == id);
        }

        public PlayerData[] GetPlayers()
        {
            return dataManager.GameData.PlayerData.ToArray();
        }
    }
}
