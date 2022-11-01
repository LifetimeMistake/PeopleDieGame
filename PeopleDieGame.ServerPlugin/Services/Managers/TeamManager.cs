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
using PeopleDieGame.NetMethods.Managers;
using PeopleDieGame.NetMethods.Models.EventArgs;

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
        public event EventHandler<TeamEventArgs> OnTeamCreated;
        public event EventHandler<TeamEventArgs> OnTeamRemoved;
        public event EventHandler<TeamEventArgs> OnBaseClaimRemoved;
        public event EventHandler<TeamInviteEventArgs> OnPlayerInvited;
        public event EventHandler<TeamInviteEventArgs> OnInvitationCancelled;
        public event EventHandler<TeamInviteEventArgs> OnInvitationAccepted;
        public event EventHandler<TeamInviteEventArgs> OnInvitationRejected;
        public event EventHandler<TeamBankEventArgs> OnBankBalanceChanged;
        public event EventHandler<TeamBankEventArgs> OnBankDepositedInto;
        public event EventHandler<TeamBankEventArgs> OnBankWithdrawnFrom;
        public event EventHandler<TeamBaseClaimEventArgs> OnBaseClaimCreated;

        private Dictionary<Team, ClaimBubble> claims = new Dictionary<Team, ClaimBubble>();
        private List<TeamInvite> invites = new List<TeamInvite>();

        public void Init()
        {
            loadoutManager.OnLoadoutRemoved += LoadoutManager_OnLoadoutRemoved;
            gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
            InviteManager.OnInviteAccepted += InviteManager_OnInviteAccepted;
            InviteManager.OnInviteRejected += InviteManager_OnInviteRejected;

            if (gameManager.GetGameState() == GameState.InGame)
                RegisterTimers();

            List<InteractableClaim> claimList = UnityEngine.Object.FindObjectsOfType<InteractableClaim>().ToList();

            foreach (InteractableClaim claim in claimList)
            {
                Team team = GetTeamByGroup((CSteamID)claim.group);
                if (team == null)
                    return;

                FieldRef<ClaimBubble> bubble = FieldRef.GetFieldRef<InteractableClaim, ClaimBubble>(claim, "bubble");
                claims.Add(team, bubble.Value);
            }
        }

        public void Dispose()
        {
            loadoutManager.OnLoadoutRemoved -= LoadoutManager_OnLoadoutRemoved;
            gameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
            InviteManager.OnInviteAccepted -= InviteManager_OnInviteAccepted;
            InviteManager.OnInviteRejected -= InviteManager_OnInviteRejected;
            UnregisterTimers();
        }

        private void InviteManager_OnInviteAccepted(object sender, InviteResponseEventArgs e)
        {
            PlayerData playerData = playerDataManager.GetPlayer(e.Player.playerID.characterID);
            if (playerData == null)
                return;

            AcceptInvite(playerData);
        }

        private void InviteManager_OnInviteRejected(object sender, InviteResponseEventArgs e)
        {
            PlayerData playerData = playerDataManager.GetPlayer(e.Player.playerID.characterID);
            if (playerData == null)
                return;

            RejectInvite(playerData);
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
                    team.SetDefaultLoadout(null);
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
                PlayerData playerData = playerDataManager.GetPlayer((ulong)steamPlayer.playerID.steamID);
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);

                if (!playerData.TeamId.HasValue)
                    return;

                Team team = GetTeam(playerData.TeamId.Value);
                if (!claims.ContainsKey(team))
                    return;

                if (Vector3.Distance(claims[team].origin, unturnedPlayer.Position) < Math.Sqrt(claims[team].sqrRadius))
                {
                    if (playerData.WalletBalance == 0)
                        return;

                    DepositIntoBank(team, playerData.WalletBalance);
                    playerDataManager.SetPlayerBalance(playerData, 0);
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
            ClaimBubble claim = claims[team];

            if (Vector3.Distance(claim.origin, point) > Math.Sqrt(claim.sqrRadius))
                return false;
            return true;
        }

        public Team CreateTeam(string name, string description = "", Loadout defaultLoadout = null, double bankFunds = 1000)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (GetTeamByName(name) != null)
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

        public Team GetTeamByName(string name, bool exactMatch = true)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (exactMatch)
                return teams.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return teams.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public Team GetTeamByGroup(CSteamID groupId)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            return teams.Values.FirstOrDefault(x => x.GroupID == groupId);
        }

        public bool JoinTeam(PlayerData player, Team team)
        {
            if (player == null || player.TeamId != null) // player doesn't exist or is already in a team
                return false;

            player.TeamId = team.Id;
            OnPlayerJoinedTeam?.Invoke(this, new TeamMembershipEventArgs(player, team));
            if (!team.LeaderId.HasValue)
            {
                SetLeader(team, player);
            }

            return true;
        }

        public bool LeaveTeam(PlayerData player)
        {
            if (player == null || player.TeamId == null) // player doesn't exist or does not belong to a team
                return false;

            Team team = GetTeam(player.TeamId.Value);
            player.TeamId = null;

            // Transfer leadership
            if (team.LeaderId == player.Id)
            {
                PlayerData otherPlayer = GetTeamMembers(team).FirstOrDefault();
                if (otherPlayer != null)
                    SetLeader(team, otherPlayer);
            }

            OnPlayerLeftTeam?.Invoke(this, new TeamMembershipEventArgs(player, team));
            return true;
        }

        public bool SetLeader(Team team, PlayerData player)
        {
            if (player == null || player.TeamId != team.Id) // player doesn't exist or does not belong to this team
                return false;

            team.SetTeamLeader(player);
            OnTeamLeaderChanged?.Invoke(this, new TeamMembershipEventArgs(player, team));
            return true;
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
            return playerDataManager.GetPlayers().Where(x => x.TeamId == team.Id).ToList();
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
            return GetTeamByName(teamNameOrId, exactMatch);
        }

        public bool InvitePlayer(Team team, PlayerData targetPlayer, float inviteTTL = 15)
        {
            if (!team.LeaderId.HasValue)
                return false;

            PlayerData callerPlayer = playerDataManager.GetPlayer(team.LeaderId.Value);
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
            InviteManager.SendInviteRequest(steamPlayer, callerPlayer.Name, team.Name, (float)inviteTTL);
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
                PlayerData leaderData = playerDataManager.GetPlayer(team.LeaderId.Value);
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

        public double GetBankBalance(Team team)
        {
            return team.BankBalance;
        }

        public void SetBankBalance(Team team, double amount)
        {
            team.SetBalance(amount);
            OnBankBalanceChanged?.Invoke(this, new TeamBankEventArgs(team, team.BankBalance));
        }

        public void DepositIntoBank(Team team, double amount)
        {
            team.Deposit(amount);
            OnBankDepositedInto?.Invoke(this, new TeamBankEventArgs(team, amount));
            OnBankBalanceChanged?.Invoke(this, new TeamBankEventArgs(team, team.BankBalance));
        }

        public void WithdrawFromBank(Team team, double amount)
        {
            team.Withdraw(amount);
            OnBankWithdrawnFrom?.Invoke(this, new TeamBankEventArgs(team, amount));
            OnBankBalanceChanged?.Invoke(this, new TeamBankEventArgs(team, team.BankBalance));
        }

        public void SetClaim(Team team, ClaimBubble bubble)
        {
            claims.Add(team, bubble);
            OnBaseClaimCreated?.Invoke(this, new TeamBaseClaimEventArgs(team, bubble));
        }

        public void RemoveClaim(Team team)
        {
            claims.Remove(team);
            OnBaseClaimRemoved?.Invoke(this, new TeamEventArgs(team));
        }

        public ClaimBubble GetClaim(Team team)
        {
            if (!claims.ContainsKey(team))
                return null;

            return claims[team];
        }

        public ClaimBubble[] GetClaims()
        {
            return claims.Values.ToArray();
        }

        public bool HasClaim(Team team)
        {
            return claims.ContainsKey(team);
        }
    }
}
