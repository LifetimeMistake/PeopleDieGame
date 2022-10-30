using PeopleDieGame.Reflection;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class GroupMappingProvider : IDisposableService
    {
        [InjectDependency]
        private TeamManager teamManager { get; set; }

        public void Init()
        {
            teamManager.OnTeamLeaderChanged += TeamManager_OnTeamLeaderChanged;
            teamManager.OnTeamCreated += TeamManager_OnTeamCreated;
            teamManager.OnTeamRemoved += TeamManager_OnTeamRemoved;
            teamManager.OnPlayerJoinedTeam += TeamManager_OnPlayerJoinedTeam;
            teamManager.OnPlayerLeftTeam += TeamManager_OnPlayerLeftTeam;
        }

        public void Dispose()
        {
            teamManager.OnTeamLeaderChanged -= TeamManager_OnTeamLeaderChanged;
            teamManager.OnTeamCreated -= TeamManager_OnTeamCreated;
            teamManager.OnTeamRemoved -= TeamManager_OnTeamRemoved;
            teamManager.OnPlayerJoinedTeam -= TeamManager_OnPlayerJoinedTeam;
            teamManager.OnPlayerLeftTeam -= TeamManager_OnPlayerLeftTeam;
        }

        private void TeamManager_OnTeamLeaderChanged(object sender, Models.EventArgs.TeamMembershipEventArgs e)
        {
            if (teamManager.GetTeamMemberCount(e.Team) > 1)
            {
                PlayerData oldLeaderData = teamManager.GetTeamMembers(e.Team)
                    .FirstOrDefault(x => UnturnedPlayer.FromCSteamID((CSteamID)x.Id).Player.quests.groupRank == EPlayerGroupRank.OWNER);

                UnturnedPlayer oldLeader = UnturnedPlayer.FromCSteamID((CSteamID)oldLeaderData.Id);
                oldLeader.Player.quests.changeRank(EPlayerGroupRank.MEMBER);
            }

            UnturnedPlayer newLeader = UnturnedPlayer.FromCSteamID((CSteamID)e.Player.Id);
            newLeader.Player.quests.changeRank(EPlayerGroupRank.OWNER);
        }

        private void TeamManager_OnTeamCreated(object sender, Models.EventArgs.TeamEventArgs e)
        {
            CSteamID teamGroupID = GroupManager.generateUniqueGroupID();
            GroupInfo teamGroupInfo = GroupManager.addGroup(teamGroupID, e.Team.Name);
            e.Team.GroupID = teamGroupID;
            GroupManager.sendGroupInfo(teamGroupInfo);
            
            if (e.Team.LeaderId.HasValue)
            {
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromCSteamID((CSteamID)e.Team.LeaderId.Value);
                unturnedPlayer.Player.quests.ServerAssignToGroup(teamGroupID, EPlayerGroupRank.OWNER, true);
            }
        }

        private void TeamManager_OnTeamRemoved(object sender, Models.EventArgs.TeamEventArgs e)
        {
            GroupManager.deleteGroup(e.Team.GroupID.Value);
            e.Team.GroupID = null;
        }

        private void TeamManager_OnPlayerJoinedTeam(object sender, Models.EventArgs.TeamMembershipEventArgs e)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromCSteamID((CSteamID)e.Player.Id);
            unturnedPlayer.Player.quests.ServerAssignToGroup(e.Team.GroupID.Value, EPlayerGroupRank.OWNER, true);
        }

        private void TeamManager_OnPlayerLeftTeam(object sender, Models.EventArgs.TeamMembershipEventArgs e)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromCSteamID((CSteamID)e.Player.Id);
            unturnedPlayer.Player.quests.leaveGroup(true);
        }
    }
}
