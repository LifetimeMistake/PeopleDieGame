using JetBrains.Annotations;
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
        public event EventHandler<TeamEventArgs> OnTeamCreated;
        public event EventHandler<TeamEventArgs> OnTeamRemoved;

        public void Init()
        { }

        public Team CreateTeam(string name, string description = "", Loadout defaultLoadout = null)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (teams.Values.Any(x => x.Name == name))
                return null;

            int teamId = teamIdProvider.GenerateId();
            int? loadoutId = null;

            if (defaultLoadout != null)
                loadoutId = defaultLoadout.Id;

            Team team = new Team(teamId, name, description, loadoutId, null, null);
            teams.Add(teamId, team);
            OnTeamCreated?.Invoke(this, new TeamEventArgs(team));

            return team;
        }

        public bool DeleteTeam(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (!teams.ContainsKey(id))
                return false;

            // Make sure all players leave the team prior to deletion.
            foreach(PlayerData data in playerDataManager.GetPlayers())
            {
                if (data.TeamId == id)
                    LeaveTeam(UnturnedPlayer.FromCSteamID((CSteamID)data.Id));
            }

            Team team = teams[id];
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

        public bool JoinTeam(UnturnedPlayer player, Team team)
        {
            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            if (playerData == null || playerData.TeamId != null) // player doesn't exist or is already in a team
                return false;

            playerData.TeamId = team.Id;
            OnPlayerJoinedTeam?.Invoke(this, new TeamMembershipEventArgs(player, team));
            return true;
        }

        public bool LeaveTeam(UnturnedPlayer player)
        {
            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            if (playerData == null || playerData.TeamId == null) // player doesn't exist or does not belong to a team
                return false;

            Team team = GetTeam(playerData.TeamId.Value);
            playerData.TeamId = null;

            OnPlayerLeftTeam?.Invoke(this, new TeamMembershipEventArgs(player, team));
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
            sb.AppendLine($"Liczba graczy w drużynie: {members} ({onlineMembers} online)");
            return sb.ToString();
        }
    }
}
