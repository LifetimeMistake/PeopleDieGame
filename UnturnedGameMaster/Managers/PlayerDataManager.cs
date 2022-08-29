using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Steamworks;
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
            PlayerData playerData = new PlayerData((ulong)player.CSteamID);
            playerList.Add(playerData);
        }

        public PlayerData GetPlayer(ulong id)
        {
            List<PlayerData> playerList = dataManager.GameData.PlayerData;
            return playerList.FirstOrDefault(x => x.Id == id);
        }

        public PlayerData GetPlayerByName(string name, bool exactMatch = true)
        {
            List<PlayerData> playerList = dataManager.GameData.PlayerData;
            if (exactMatch)
                return playerList.FirstOrDefault(x => UnturnedPlayer.FromCSteamID((CSteamID)x.Id).DisplayName.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return playerList.FirstOrDefault(x => UnturnedPlayer.FromCSteamID((CSteamID)x.Id).DisplayName.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public PlayerData[] GetPlayers()
        {
            return dataManager.GameData.PlayerData.ToArray();
        }

        public PlayerData ResolvePlayer(string playerNameOrId, bool exactMatch)
        {
            ulong id;
            if (ulong.TryParse(playerNameOrId, out id))
            {
                // might be an ID but idk
                PlayerData playerData = GetPlayer(id);
                if (playerData != null)
                    return playerData;
            }

            // otherwise try matching by name
            return GetPlayerByName(playerNameOrId, exactMatch); 
        }
    }
}
