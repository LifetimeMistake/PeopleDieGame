using Rocket.Unturned.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
using PeopleDieGame.NetMethods.RPCs;
using PeopleDieGame.NetMethods.Models;
using Rocket.Unturned.Player;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class PlayerDataManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }

        private Dictionary<ulong, UnturnedPlayer> playerConnections;

        public event EventHandler<PlayerEventArgs> OnBalanceUpdated;
        public event EventHandler<PlayerEventArgs> OnBountyUpdated;
        public event EventHandler<PlayerEventArgs> OnBioUpdated;

        public void Init()
        {
            playerConnections = new Dictionary<ulong, UnturnedPlayer>();
            UnturnedEvents.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
            UnturnedEvents.Instance.OnPlayerDisconnected += Instance_OnPlayerDisconnected;
        }

        public void Dispose()
        {
            UnturnedEvents.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
            UnturnedEvents.Instance.OnPlayerDisconnected -= Instance_OnPlayerDisconnected;
            playerConnections = null;
        }

        private void Instance_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            Dictionary<ulong, PlayerData> players = dataManager.GameData.PlayerData;
            PlayerData playerData = GetData((ulong)player.CSteamID);

            if (playerData == null)
            {
                // register a new player
                playerData = new PlayerData((ulong)player.CSteamID, player.DisplayName);
                players.Add(playerData.Id, playerData);
            }
            else
            {
                playerData.Name = player.DisplayName;
            }

            SendDataUpdate(playerData);
        }

        private void Instance_OnPlayerDisconnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            playerConnections.Remove((ulong)player.CSteamID);
        }

        private void SendDataUpdate(PlayerData data)
        {
            UnturnedPlayer player = playerConnections[data.Id];
            PlayerInfo info = new PlayerInfo(data.Id, data.Name, data.Bio, data.TeamId, data.WalletBalance, data.Bounty);
            ClientDataRPC.UpdatePlayerInfo(player.SteamPlayer(), info);
        }

        public PlayerData GetData(ulong id)
        {
            Dictionary<ulong, PlayerData> players = dataManager.GameData.PlayerData;
            if (!players.ContainsKey(id))
                return null;

            return players[id];
        }

        public PlayerData GetData(string name, bool exactMatch = true)
        {
            Dictionary<ulong, PlayerData> players = dataManager.GameData.PlayerData;
            if (exactMatch)
                return players.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return players.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public UnturnedPlayer GetPlayerConnection(ulong id)
        {
            if (!playerConnections.ContainsKey(id))
                return null;

            return playerConnections[id];
        }

        public PlayerData[] GetAllData()
        {
            return dataManager.GameData.PlayerData.Values.ToArray();
        }

        public int GetRegisteredPlayerCount()
        {
            return dataManager.GameData.PlayerData.Count;
        }

        public PlayerData ResolvePlayer(string playerNameOrId, bool exactMatch)
        {
            ulong id;
            if (ulong.TryParse(playerNameOrId, out id))
            {
                // might be an ID but idk
                PlayerData playerData = GetData(id);
                if (playerData != null)
                    return playerData;
            }

            // otherwise try matching by name
            return GetData(playerNameOrId, exactMatch);
        }

        public void UpdateTeamMembership(PlayerData playerData, int? teamId)
        {
            playerData.TeamId = teamId;
            SendDataUpdate(playerData);
        }

        public void AddBalance(PlayerData playerData, float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            UpdateBalance(playerData, playerData.WalletBalance + amount);
        }

        public void RemoveBalance(PlayerData playerData, float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            float newBalance = playerData.WalletBalance - amount;
            if (newBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            UpdateBalance(playerData, newBalance);
        }

        public void UpdateBalance(PlayerData playerData, float amount)
        {
            playerData.WalletBalance = amount;
            SendDataUpdate(playerData);
            OnBalanceUpdated?.Invoke(this, new PlayerEventArgs(playerData));
        }

        public void AddBounty(PlayerData playerData, float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            UpdateBounty(playerData, playerData.Bounty + amount);
        }

        public void RemoveBounty(PlayerData playerData, float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            float newBounty = playerData.Bounty - amount;
            if (newBounty < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            UpdateBounty(playerData, newBounty);
        }

        public void UpdateBounty(PlayerData playerData, float amount)
        {
            playerData.Bounty = amount;
            SendDataUpdate(playerData);
            OnBountyUpdated?.Invoke(this, new PlayerEventArgs(playerData));
        }

        public void UpdateBio(PlayerData playerData, string bio)
        {
            playerData.Bio = bio;
            SendDataUpdate(playerData);
            OnBioUpdated?.Invoke(this, new PlayerEventArgs(playerData));
        }
    }
}
