using JetBrains.Annotations;
using Rocket.Unturned.Chat;
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
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class TeamManager : IManager
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private LoadoutManager loadoutManager { get; set; }
        [InjectDependency]
        private TeamIdProvider teamIdProvider { get; set; }

        public event EventHandler<TeamMembershipEventArgs> OnPlayerJoinedTeam;
        public event EventHandler<TeamMembershipEventArgs> OnPlayerLeftTeam;
        public event EventHandler<TeamMembershipEventArgs> OnTeamLeaderChanged;
        public event EventHandler<TeamEventArgs> OnTeamCreated;
        public event EventHandler<TeamEventArgs> OnTeamRemoved;
        public event EventHandler<TeamInvitationEventArgs> OnPlayerInvited;
        public event EventHandler<TeamInvitationEventArgs> OnInvitationCancelled;
        public event EventHandler<TeamInvitationEventArgs> OnInvitationAccepted;
        public event EventHandler<TeamInvitationEventArgs> OnInvitationRejected;
        public event EventHandler<TeamEventArgs> OnBankBalanceChanged;
        public event EventHandler<TeamEventArgs> OnBankDepositedInto;
        public event EventHandler<TeamEventArgs> OnBankWithdrawnFrom;

        public void Init()
        { }

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
            if (!teams.ContainsKey(id))
                return false;

            Team team = teams[id];
            teams.Remove(id);

            // Make sure all players leave the team prior to deletion.
            foreach (PlayerData data in playerDataManager.GetPlayers())
            {
                if (data.TeamId == id)
                    LeaveTeam(data);
            }

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

        public int GetTeamPlayerCount(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (!teams.ContainsKey(id))
                return 0;

            int playerCount = 0;
            foreach(PlayerData playerData in playerDataManager.GetPlayers())
            {
                if(playerData.TeamId == id)
                    playerCount++;
            }

            return playerCount;
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
                PlayerData otherPlayer = playerDataManager.GetPlayers().FirstOrDefault(x => x.TeamId == team.Id);
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

        public bool InvitePlayer(Team team, PlayerData targetPlayerData, int invitationTTL = 30)
        {
            if (!team.LeaderId.HasValue)
                return false;

            PlayerData callerPlayerData = playerDataManager.GetPlayer(team.LeaderId.Value);
            if (callerPlayerData == null)
                return false; // failed to get team leader

            if (targetPlayerData.TeamId.HasValue) // player already belongs to a team
                return false;

            if (team.GetInvitations().Any(x => x.TargetId == targetPlayerData.Id))
                return false; // player already has a pending invitation

            TeamInvitation teamInvitation = new TeamInvitation(callerPlayerData.Id, targetPlayerData.Id, DateTime.Now, TimeSpan.FromSeconds(invitationTTL));
            team.Invitations.Add(teamInvitation);
            OnPlayerInvited?.Invoke(this, new TeamInvitationEventArgs(team, teamInvitation));
            return true;
        }

        public bool CancelInvitation(Team team, PlayerData targetPlayerData)
        {
            TeamInvitation teamInvitation = team.GetInvitations().FirstOrDefault(x => x.TargetId == targetPlayerData.Id);
            if (teamInvitation != null)
            {
                team.RemoveInvitation(targetPlayerData.Id);
                OnInvitationCancelled?.Invoke(this, new TeamInvitationEventArgs(team, teamInvitation));
                return true;
            }

            return false;
        }

        public bool AcceptInvitation(Team team, PlayerData targetPlayerData)
        {
            TeamInvitation teamInvitation = team.GetInvitations().FirstOrDefault(x => x.TargetId == targetPlayerData.Id);
            if (teamInvitation != null)
            {
                if (!JoinTeam(targetPlayerData, team))
                    return false;

                team.RemoveInvitation(targetPlayerData.Id);
                OnInvitationAccepted?.Invoke(this, new TeamInvitationEventArgs(team, teamInvitation));
                return true;
            }

            return false;
        }

        public bool RejectInvitation(Team team, PlayerData targetPlayerData)
        {
            TeamInvitation teamInvitation = team.GetInvitations().FirstOrDefault(x => x.TargetId == targetPlayerData.Id);
            if (teamInvitation != null)
            {
                team.RemoveInvitation(targetPlayerData.Id);
                OnInvitationRejected?.Invoke(this, new TeamInvitationEventArgs(team, teamInvitation));
                return true;
            }

            return false;
        }

        public string GetTeamSummary(Team team)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\"{team.Name}\"");

            if (team.Description != "")
                sb.AppendLine($"Opis: \"{team.Description}\"");
            else
                sb.AppendLine("Opis: Brak opisu");

            List<PlayerData> members = new List<PlayerData>();
            foreach(PlayerData playerData in playerDataManager.GetPlayers())
            {
                if (playerData.TeamId == team.Id)
                    members.Add(playerData);
            }

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
            OnBankBalanceChanged?.Invoke(this, new TeamEventArgs(team));
        }

        public void DepositIntoBank(Team team, double amount)
        {
            team.Deposit(amount);
            OnBankBalanceChanged?.Invoke(this, new TeamEventArgs(team));
            OnBankDepositedInto?.Invoke(this, new TeamEventArgs(team));
        }

        public void WithdrawFromBank(Team team, double amount)
        {
            team.Withdraw(amount);
            OnBankBalanceChanged?.Invoke(this, new TeamEventArgs(team));
            OnBankWithdrawnFrom?.Invoke(this, new TeamEventArgs(team));
        }
    }
}
