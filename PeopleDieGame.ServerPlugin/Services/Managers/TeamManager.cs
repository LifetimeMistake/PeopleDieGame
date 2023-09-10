using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
using PeopleDieGame.ServerPlugin.Services.Providers;
using Rocket.Unturned.Player;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Enums;
using UnityEngine;
using PeopleDieGame.Reflection;
using PeopleDieGame.NetMethods.RPCs;
using PeopleDieGame.NetMethods.Models.EventArgs;
using System.Runtime.InteropServices.WindowsRuntime;
using PeopleDieGame.NetMethods.Models;
using static Rocket.Unturned.Events.UnturnedPlayerEvents;
using Rocket.Unturned.Events;
using Rocket.Core.Steam;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class TeamManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private TeamIdProvider teamIdProvider { get; set; }
        [InjectDependency]
        private GameManager gameManager { get; set; }
        [InjectDependency]
        private LoadoutManager loadoutManager { get; set; }
        [InjectDependency]
        private TimerManager timerManager { get; set; }

        public event EventHandler<TeamMembershipEventArgs> OnPlayerJoinedTeam;
        public event EventHandler<TeamMembershipEventArgs> OnPlayerLeftTeam;
        public event EventHandler<TeamMembershipEventArgs> OnTeamLeaderChanged;
        public event EventHandler<TeamLoadoutChangedEventArgs> OnTeamLoadoutChanged;
        public event EventHandler<TeamEventArgs> OnTeamCreated;
        public event EventHandler<TeamEventArgs> OnTeamRemoved;
        public event EventHandler<TeamEventArgs> OnBaseClaimRemoved;
        public event EventHandler<TeamInviteEventArgs> OnPlayerInvited;
        public event EventHandler<TeamInviteEventArgs> OnInvitationCancelled;
        public event EventHandler<TeamInviteEventArgs> OnInvitationAccepted;
        public event EventHandler<TeamInviteEventArgs> OnInvitationRejected;
        public event EventHandler<TeamBankEventArgs> OnBankBalanceChanged;
        public event EventHandler<TeamBaseClaimEventArgs> OnBaseClaimCreated;
        public event EventHandler<PlayerEventArgs> OnTeamDataSynced;

        private Dictionary<int, ClaimBubble> claims = new Dictionary<int, ClaimBubble>();
        private List<TeamInvite> invites = new List<TeamInvite>();

        public void Init()
        {
            loadoutManager.OnLoadoutRemoved += LoadoutManager_OnLoadoutRemoved;
            gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
            InviteRPC.OnInviteAccepted += InviteManager_OnInviteAccepted;
            InviteRPC.OnInviteRejected += InviteManager_OnInviteRejected;
            playerDataManager.OnPlayerDataSynced += PlayerDataManager_OnPlayerDataSynced;

            if (gameManager.GetGameState() == GameState.InGame)
                RegisterTimers();

            List<InteractableClaim> claimList = UnityEngine.Object.FindObjectsOfType<InteractableClaim>().ToList();

            foreach (InteractableClaim claim in claimList)
            {
                Team team = GetTeam((CSteamID)claim.group);
                if (team == null)
                    return;

                FieldRef<ClaimBubble> bubble = FieldRef.GetFieldRef<InteractableClaim, ClaimBubble>(claim, "bubble");
                claims.Add(team.Id, bubble.Value);
            }
        }

        private void PlayerDataManager_OnPlayerDataSynced(object sender, PlayerEventArgs e)
        {
            PlayerData playerData = e.Player;
            if (!playerData.TeamId.HasValue)
                return;

            Team team = GetTeam(playerData.TeamId.Value);
            if (team == null)
                throw new Exception("Could not get team");

            TeamInfo info = BuildTeamInfo(team);
            UnturnedPlayer player = playerDataManager.GetPlayerConnection(playerData.Id);
            if (player == null)
                throw new Exception("Could not obtain player connection");

            ClientDataRPC.UpdateTeamInfo(player.SteamPlayer(), info);
            OnTeamDataSynced?.Invoke(this, e);
        }

        public void Dispose()
        {
            loadoutManager.OnLoadoutRemoved -= LoadoutManager_OnLoadoutRemoved;
            gameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
            InviteRPC.OnInviteAccepted -= InviteManager_OnInviteAccepted;
            InviteRPC.OnInviteRejected -= InviteManager_OnInviteRejected;
            playerDataManager.OnPlayerDataSynced -= PlayerDataManager_OnPlayerDataSynced;
            UnregisterTimers();
        }

        private void InviteManager_OnInviteAccepted(object sender, InviteResponseEventArgs e)
        {
            PlayerData playerData = playerDataManager.GetData(e.Player.playerID.characterID);
            if (playerData == null)
                return;

            AcceptInvite(playerData);
        }

        private void InviteManager_OnInviteRejected(object sender, InviteResponseEventArgs e)
        {
            PlayerData playerData = playerDataManager.GetData(e.Player.playerID.characterID);
            if (playerData == null)
                return;

            RejectInvite(playerData);
        }

        private TeamInfo BuildTeamInfo(Team team)
        {
            PlayerData leaderData = null;
            if (team.LeaderId.HasValue)
            {
                leaderData = playerDataManager.GetData(team.LeaderId.Value);
                if (leaderData == null)
                {
                    throw new Exception("Could not obtain team leader data");
                }
            }

            ClaimInfo? claim = null;
            if (claims.ContainsKey(team.Id))
            {
                ClaimBubble claimBubble = claims[team.Id];
                claim = new ClaimInfo(claimBubble.origin, claimBubble.sqrRadius);
            }

            string leaderName = leaderData?.Name;
            TeamInfo info = new TeamInfo(team.Id, team.Name, team.Description, team.BankBalance, team.LeaderId, leaderName, claim);
            return info;
        }

        private void SendDataUpdate(Team team)
        {
            TeamInfo info;
            try
            {
                info = BuildTeamInfo(team);
            }
            catch(Exception ex)
            {
                Debug.LogWarning($"Failed to send team update: {ex.Message}");
                return;
            }

            foreach (PlayerData playerData in GetOnlineTeamMembers(team))
            {
                UnturnedPlayer player = playerDataManager.GetPlayerConnection(playerData.Id);
                if (player == null)
                {
                    Debug.LogWarning($"Failed to send team update to player {playerData.Id}, {playerData.Name}: could not find player connection");
                    continue;
                }

                ClientDataRPC.UpdateTeamInfo(player.SteamPlayer(), info);
            }
        }

        private void RegisterTimers()
        {
            UnregisterTimers();
            timerManager.Register(AutoDeposit, 120);
        }

        private void UnregisterTimers()
        {
            timerManager.Unregister(AutoDeposit);
        }

        private void LoadoutManager_OnLoadoutRemoved(object sender, LoadoutEventArgs e)
        {
            foreach (Team team in GetTeams())
            {
                if (team.DefaultLoadoutId == e.Loadout.Id)
                    UpdateLoadout(team, null);
            }
        }

        private void GameManager_OnGameStateChanged(object sender, EventArgs e)
        {
            GameState gameState = gameManager.GetGameState();
            if (gameState == GameState.InGame)
            {
                RegisterTimers();
            }
            else
            {
                UnregisterTimers();
            }
        }

        private void AutoDeposit()
        {
            foreach (SteamPlayer steamPlayer in Provider.clients)
            {
                PlayerData playerData = playerDataManager.GetData((ulong)steamPlayer.playerID.steamID);
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);

                if (!playerData.TeamId.HasValue)
                    return;

                Team team = GetTeam(playerData.TeamId.Value);
                if (!claims.ContainsKey(team.Id))
                    return;

                if (IsInClaimRadius(team, unturnedPlayer.Position))
                {
                    if (playerData.WalletBalance == 0)
                        return;

                    AddBalance(team, playerData.WalletBalance);
                    playerDataManager.UpdateBalance(playerData, 0);
                    ChatHelper.Say(playerData, $"Środki z twojego portfela trafiły do banku twojej drużyny");
                }
            }
        }

        private void ProcessExpiredInvites()
        {
            invites.RemoveAll(x => x.IsExpired());
        }

        public bool IsInClaimRadius(Team team, Vector3S point)
        {
            ClaimBubble claim = claims[team.Id];
            float distance = Vector3.Distance(claim.origin, point);
            return (distance * distance < claim.sqrRadius);
        }

        public ClaimBubble GetClaim(Team team)
        {
            if (!claims.ContainsKey(team.Id))
                return null;

            return claims[team.Id];
        }

        public ClaimBubble[] GetClaims()
        {
            return claims.Values.ToArray();
        }

        public bool HasClaim(Team team)
        {
            return claims.ContainsKey(team.Id);
        }

        public Team CreateTeam(string name, string description = "", Loadout defaultLoadout = null, float bankFunds = 1000)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (GetTeam(name) != null)
                return null;

            int teamId = teamIdProvider.GenerateId();
            int? loadoutId = null;

            if (defaultLoadout != null)
                loadoutId = defaultLoadout.Id;

            Team team = new Team(teamId, name, description, loadoutId, null, null, bankFunds);
            teams.Add(teamId, team);

            OnTeamCreated?.Invoke(this, new TeamEventArgs(team));
            return team;
        }

        public bool DeleteTeam(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            Team team = GetTeam(id);
            if (team == null)
                return false;

            // Make sure all players leave the team prior to deletion.
            GetTeamMembers(team).ForEach(x => LeaveTeam(x));

            teams.Remove(id);
            OnTeamRemoved?.Invoke(this, new TeamEventArgs(team));
            return true;
        }

        public Team GetTeam(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (!teams.ContainsKey(id))
                return null;

            return teams[id];
        }

        public Team GetTeam(string name, bool exactMatch = true)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (exactMatch)
                return teams.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return teams.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public Team GetTeam(CSteamID groupId)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            return teams.Values.FirstOrDefault(x => x.GroupID == groupId);
        }

        public Team[] GetTeams()
        {
            return dataManager.GameData.Teams.Values.ToArray();
        }

        public int GetTeamCount()
        {
            return dataManager.GameData.Teams.Count;
        }

        public List<PlayerData> GetTeamMembers(Team team)
        {
            return playerDataManager.GetAllData().Where(x => x.TeamId == team.Id).ToList();
        }

        public List<PlayerData> GetOnlineTeamMembers(Team team)
        {
            return GetTeamMembers(team).Where(x => Provider.clients.Any(y => (ulong)y.playerID.steamID == x.Id)).ToList();
        }

        public int GetTeamMemberCount(Team team)
        {
            return GetTeamMembers(team).Count;
        }

        public int GetOnlineTeamMemberCount(Team team)
        {
            return GetOnlineTeamMembers(team).Count;
        }

        public int GetTeamMemberCount(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (!teams.ContainsKey(id))
                return 0;

            Team team = teams[id];
            return GetTeamMemberCount(team);
        }

        public TeamInvite[] GetInvites()
        {
            ProcessExpiredInvites();
            return invites.ToArray();
        }

        public TeamInvite GetInvite(PlayerData targetPlayer)
        {
            ProcessExpiredInvites();
            return invites.FirstOrDefault(x => x.Target == targetPlayer);
        }

        public bool JoinTeam(PlayerData playerData, Team team)
        {
            if (playerData == null || playerData.TeamId != null) // player doesn't exist or is already in a team
                return false;

            playerDataManager.UpdateTeamMembership(playerData, team.Id);
            OnPlayerJoinedTeam?.Invoke(this, new TeamMembershipEventArgs(playerData, team));
            if (!team.LeaderId.HasValue)
            {
                UpdateLeader(team, playerData);
            }

            try
            {
                TeamInfo info = BuildTeamInfo(team);
                UnturnedPlayer player = playerDataManager.GetPlayerConnection(playerData.Id);
                if (player == null)
                    throw new Exception("Could not obtain player connection");

                ClientDataRPC.UpdateTeamInfo(player.SteamPlayer(), info);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to send join team update: {ex.Message}");
            }

            OnPlayerJoinedTeam?.Invoke(this, new TeamMembershipEventArgs(playerData, team));
            return true;
        }

        public bool LeaveTeam(PlayerData playerData)
        {
            if (playerData == null || playerData.TeamId == null) // player doesn't exist or does not belong to a team
                return false;

            Team team = GetTeam(playerData.TeamId.Value);
            playerDataManager.UpdateTeamMembership(playerData, null);

            // Transfer leadership
            if (team.LeaderId == playerData.Id)
            {
                PlayerData otherPlayer = GetTeamMembers(team).FirstOrDefault();
                UpdateLeader(team, otherPlayer);
            }

            UnturnedPlayer player = playerDataManager.GetPlayerConnection(playerData.Id);
            if (player != null)
                ClientDataRPC.UpdateTeamInfo(player.SteamPlayer(), null);
            else
                Debug.LogWarning("Failed to send leave team update: Could not obtain player connection");

            OnPlayerLeftTeam?.Invoke(this, new TeamMembershipEventArgs(playerData, team));
            return true;
        }

        public void UpdateName(Team team, string name)
        {
            team.Name = name ?? throw new ArgumentNullException(nameof(name));
            SendDataUpdate(team);
            // add event??
        }
        
        public void UpdateDescription(Team team, string description)
        {
            team.Description = description ?? throw new ArgumentNullException(nameof(description));
            SendDataUpdate(team);
            // add event??
        }

        public bool UpdateLeader(Team team, PlayerData playerData)
        {
            team.LeaderId = playerData?.Id;
            SendDataUpdate(team);
            OnTeamLeaderChanged?.Invoke(this, new TeamMembershipEventArgs(playerData, team));
            return true;
        }

        public bool UpdateLoadout(Team team, Loadout loadout)
        {
            if (team == null)
                return false;

            team.DefaultLoadoutId = loadout?.Id;
            SendDataUpdate(team);
            OnTeamLoadoutChanged?.Invoke(this, new TeamLoadoutChangedEventArgs(team, loadout));
            return true;
        }

        public void AddBalance(Team team, float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            UpdateBalance(team, team.BankBalance + amount);
        }

        public void RemoveBalance(Team team, float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            float newBalance = team.BankBalance - amount;
            if (newBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            UpdateBalance(team, newBalance);
        }

        public void UpdateBalance(Team team, float amount)
        {
            team.BankBalance = amount;
            SendDataUpdate(team);
            OnBankBalanceChanged?.Invoke(this, new TeamBankEventArgs(team, team.BankBalance));
        }

        public bool UpdateClaim(Team team, ClaimBubble bubble)
        {
            if (claims.ContainsKey(team.Id))
                return false;

            claims.Add(team.Id, bubble);
            SendDataUpdate(team);
            OnBaseClaimCreated?.Invoke(this, new TeamBaseClaimEventArgs(team, bubble));
            return true;
        }

        public void RemoveClaim(Team team)
        {
            if (!claims.ContainsKey(team.Id))
                return;

            claims.Remove(team.Id);
            SendDataUpdate(team);
            OnBaseClaimRemoved?.Invoke(this, new TeamEventArgs(team));
        }

        public Team ResolveTeam(string teamNameOrId, bool exactMatch)
        {
            int id;
            if (int.TryParse(teamNameOrId, out id))
            {
                // might be an ID but idk
                Team teamData = GetTeam(id);
                if (teamData != null)
                    return teamData;
            }

            // otherwise try matching by name
            return GetTeam(teamNameOrId, exactMatch);
        }

        public bool InvitePlayer(Team team, PlayerData targetPlayer, float inviteTTL = 15)
        {
            if (!team.LeaderId.HasValue)
                return false;

            PlayerData callerPlayer = playerDataManager.GetData(team.LeaderId.Value);
            if (callerPlayer == null)
                return false; // failed to get team leader

            if (targetPlayer.TeamId.HasValue) // player already belongs to a team
                return false;

            if (GetInvites().Any(x => x.Target == targetPlayer))
                return false; // player already has a pending invite


            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromCSteamID((CSteamID)targetPlayer.Id);
            if (unturnedPlayer == null)
                return false;

            SteamPlayer steamPlayer = unturnedPlayer.SteamPlayer();
            if (steamPlayer == null)
                return false;

            TeamInvite invite = new TeamInvite(callerPlayer, targetPlayer, team, DateTime.Now, TimeSpan.FromSeconds(inviteTTL));
            invites.Add(invite);
            InviteRPC.SendInviteRequest(steamPlayer, callerPlayer.Name, team.Name, (float)inviteTTL);
            OnPlayerInvited?.Invoke(this, new TeamInviteEventArgs(invite));
            return true;
        }

        public bool CancelInvite(PlayerData targetPlayer)
        {
            TeamInvite invite = GetInvite(targetPlayer);
            if (invite == null)
                return false;

            invites.Remove(invite);
            OnInvitationCancelled?.Invoke(this, new TeamInviteEventArgs(invite));
            return true;
        }

        public bool AcceptInvite(PlayerData targetPlayer)
        {
            TeamInvite invite = GetInvite(targetPlayer);
            if (invite == null)
                return false;

            if (!JoinTeam(targetPlayer, invite.Team))
                return false;

            invites.Remove(invite);
            OnInvitationAccepted?.Invoke(this, new TeamInviteEventArgs(invite));
            return true;
        }

        public bool RejectInvite(PlayerData targetPlayer)
        {
            TeamInvite invite = GetInvite(targetPlayer);
            if (invite == null)
                return false;

            invites.Remove(invite);
            OnInvitationRejected?.Invoke(this, new TeamInviteEventArgs(invite));
            return true;
        }

        public string GetTeamSummary(Team team)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\"{team.Name}\"");

            if (team.Description != "")
                sb.AppendLine($"Opis: \"{team.Description}\"");
            else
                sb.AppendLine("Opis: Brak opisu");

            if (team.LeaderId.HasValue)
            {
                PlayerData leaderData = playerDataManager.GetData(team.LeaderId.Value);
                sb.AppendLine($"Lider: \"{leaderData.Name}\"");
            }
            else
                sb.AppendLine("Lider: Brak");


            List<PlayerData> members = GetTeamMembers(team);
            int onlineMembers = Provider.clients.Count(x => members.Any(m => (CSteamID)m.Id == x.playerID.steamID));
            sb.AppendLine($"Liczba graczy w drużynie: {members.Count} ({onlineMembers} online)");
            sb.AppendLine($"Członkowie drużyny: {string.Join(", ", members.Select(x => x.Name))}");
            return sb.ToString();
        }
    }
}
