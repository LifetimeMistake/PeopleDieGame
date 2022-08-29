using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class PlayerDataManager : IDisposableManager
    {
        [InjectDependency]
        private DataManager dataManager{ get; set; }
        [InjectDependency]
        private TeamManager teamManager{ get; set; }

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
            PlayerData playerData = playerList.FirstOrDefault(x => x.Id == (ulong)player.CSteamID);

            if(playerData == null)
            {
                // register a new player
                playerData = new PlayerData((ulong)player.CSteamID, player.CharacterName);
                playerList.Add(playerData);
            }
            else
            {
                playerData.SetName(player.CharacterName);
            }
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
                return playerList.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return playerList.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public PlayerData[] GetPlayers()
        {
            return dataManager.GameData.PlayerData.ToArray();
        }

        public int GetPlayerCount()
        {
            return dataManager.GameData.PlayerData.Count;
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
        
        public string GetPlayerSummary(PlayerData playerData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Profil gracza \"{playerData.Name}\"");

            if (playerData.TeamId.HasValue)
            {
                Team team = teamManager.GetTeam(playerData.TeamId.Value);
                sb.AppendLine($"Drużyna: \"{team.Name}\"");
            }
            else
            {
                sb.AppendLine($"Drużyna: Brak drużyny");
            }

            if (playerData.Bio != "")
                sb.AppendLine($"Bio: \"{playerData.Bio}\"");

            return sb.ToString();
        }
    }
}
