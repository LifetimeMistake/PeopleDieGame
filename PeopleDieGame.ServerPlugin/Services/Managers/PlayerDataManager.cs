using Rocket.Unturned.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class PlayerDataManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }

        public event EventHandler<PlayerEventArgs> OnWalletBalanceChanged;
        public event EventHandler<PlayerEventArgs> OnWalletDepositedInto;
        public event EventHandler<PlayerEventArgs> OnWalletWithdrawnFrom;
        public event EventHandler<PlayerEventArgs> OnBountyAdded;
        public event EventHandler<PlayerEventArgs> OnBountyReset;

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
            Dictionary<ulong, PlayerData> players = dataManager.GameData.PlayerData;
            PlayerData playerData = GetPlayer((ulong)player.CSteamID);

            if (playerData == null)
            {
                // register a new player
                playerData = new PlayerData((ulong)player.CSteamID, player.DisplayName);
                players.Add(playerData.Id, playerData);
            }
            else
            {
                playerData.SetName(player.DisplayName);
            }
        }

        public PlayerData GetPlayer(ulong id)
        {
            Dictionary<ulong, PlayerData> players = dataManager.GameData.PlayerData;
            if (!players.ContainsKey(id))
                return null;

            return players[id];
        }

        public PlayerData GetPlayerByName(string name, bool exactMatch = true)
        {
            Dictionary<ulong, PlayerData> players = dataManager.GameData.PlayerData;
            if (exactMatch)
                return players.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return players.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public PlayerData[] GetPlayers()
        {
            return dataManager.GameData.PlayerData.Values.ToArray();
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

        public float GetPlayerBalance(PlayerData playerData)
        {
            return playerData.WalletBalance;
        }

        public void SetPlayerBalance(PlayerData playerData, float amount)
        {
            playerData.SetBalance(amount);
            OnWalletBalanceChanged?.Invoke(this, new PlayerEventArgs(playerData));
        }

        public void DepositIntoWallet(PlayerData playerData, float amount)
        {
            playerData.Deposit(amount);
            OnWalletBalanceChanged?.Invoke(this, new PlayerEventArgs(playerData));
            OnWalletDepositedInto?.Invoke(this, new PlayerEventArgs(playerData));
        }

        public void WithdrawFromWallet(PlayerData playerData, float amount)
        {
            playerData.Withdraw(amount);
            OnWalletBalanceChanged?.Invoke(this, new PlayerEventArgs(playerData));
            OnWalletWithdrawnFrom?.Invoke(this, new PlayerEventArgs(playerData));
        }

        public void AddBounty(PlayerData playerData, float amount)
        {
            playerData.AddBounty(amount);
            OnBountyAdded?.Invoke(this, new PlayerEventArgs(playerData));
        }

        public void ResetBounty(PlayerData playerData)
        {
            playerData.ResetBounty();
            OnBountyReset?.Invoke(this, new PlayerEventArgs(playerData));
        }
    }
}
