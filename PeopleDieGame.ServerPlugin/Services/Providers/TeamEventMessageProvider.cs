using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class TeamEventMessageProvider : IDisposableService
    {
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }

        public void Init()
        {
            teamManager.OnPlayerInvited += TeamManager_OnPlayerInvited;
            teamManager.OnInvitationAccepted += TeamManager_OnInvitationAccepted;
            teamManager.OnInvitationRejected += TeamManager_OnInvitationRejected;
            teamManager.OnInvitationCancelled += TeamManager_OnInvitationCancelled;
            teamManager.OnPlayerJoinedTeam += TeamManager_OnPlayerJoinedTeam;
            teamManager.OnPlayerLeftTeam += TeamManager_OnPlayerLeftTeam;
            teamManager.OnTeamLeaderChanged += TeamManager_OnTeamLeaderChanged;
            teamManager.OnBankDepositedInto += TeamManager_OnBankDepositedInto;
            teamManager.OnBankWithdrawnFrom += TeamManager_OnBankWithdrawnFrom;
            teamManager.OnBankBalanceChanged += TeamManager_OnBankBalanceChanged;
        }

        public void Dispose()
        {
            teamManager.OnPlayerInvited -= TeamManager_OnPlayerInvited;
            teamManager.OnInvitationAccepted -= TeamManager_OnInvitationAccepted;
            teamManager.OnInvitationRejected -= TeamManager_OnInvitationRejected;
            teamManager.OnInvitationCancelled -= TeamManager_OnInvitationCancelled;
            teamManager.OnPlayerJoinedTeam -= TeamManager_OnPlayerJoinedTeam;
            teamManager.OnPlayerLeftTeam -= TeamManager_OnPlayerLeftTeam;
            teamManager.OnTeamLeaderChanged -= TeamManager_OnTeamLeaderChanged;
            teamManager.OnBankDepositedInto -= TeamManager_OnBankDepositedInto;
            teamManager.OnBankWithdrawnFrom -= TeamManager_OnBankWithdrawnFrom;
            teamManager.OnBankBalanceChanged -= TeamManager_OnBankBalanceChanged;
        }

        private void TeamManager_OnBankWithdrawnFrom(object sender, Models.EventArgs.TeamBankEventArgs e)
        {
            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(e.Team))
            {
                ChatHelper.Say(player, $"Z konta bankowego twojej drużyny zostało wypłacone ${e.Amount}");
            }
        }

        private void TeamManager_OnBankBalanceChanged(object sender, Models.EventArgs.TeamBankEventArgs e)
        {
            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(e.Team))
            {
                ChatHelper.Say(player, $"Nowy stan konta bankowego twojej drużyny: ${e.Amount}");
            }
        }

        private void TeamManager_OnBankDepositedInto(object sender, Models.EventArgs.TeamBankEventArgs e)
        {
            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(e.Team))
            {
                ChatHelper.Say(player, $"Do konta bankowego twojej drużyny zostało wpłacone ${e.Amount}");
            }
        }

        private void TeamManager_OnTeamLeaderChanged(object sender, Models.EventArgs.TeamMembershipEventArgs e)
        {
            ChatHelper.Say(e.Player, $"Zostałeś nowym liderem drużyny {e.Team.Name}");
            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(e.Team))
            {
                if (player == e.Player)
                    continue;

                ChatHelper.Say(player, $"Gracz {e.Player.Name} został nowym liderem twojej drużyny");
            }
        }

        private void TeamManager_OnPlayerLeftTeam(object sender, Models.EventArgs.TeamMembershipEventArgs e)
        {
            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(e.Team))
            {
                ChatHelper.Say(player, $"Gracz {e.Player.Name} wyszedł z twojej drużyny");
            }
        }

        private void TeamManager_OnPlayerJoinedTeam(object sender, Models.EventArgs.TeamMembershipEventArgs e)
        {
            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(e.Team))
            {
                if (player == e.Player)
                    continue;

                ChatHelper.Say(player, $"Gracz {e.Player.Name} dołączył do twojej drużyny");
            }
        }

        private void TeamManager_OnInvitationCancelled(object sender, Models.EventArgs.TeamInvitationEventArgs e)
        {
            PlayerData inviter = playerDataManager.GetPlayer(e.TeamInvitation.InviterId);
            PlayerData target = playerDataManager.GetPlayer(e.TeamInvitation.TargetId);

            ChatHelper.Say(target, $"Zaproszenie gracza {inviter.Name} do drużyny {e.Team.Name} zostało anulowane");
        }

        private void TeamManager_OnInvitationRejected(object sender, Models.EventArgs.TeamInvitationEventArgs e)
        {
            PlayerData inviter = playerDataManager.GetPlayer(e.TeamInvitation.InviterId);
            PlayerData target = playerDataManager.GetPlayer(e.TeamInvitation.TargetId);

            ChatHelper.Say(inviter, $"Gracz {target.Name} odrzucił twoje zaproszenie do drużyny");
        }

        private void TeamManager_OnInvitationAccepted(object sender, Models.EventArgs.TeamInvitationEventArgs e)
        {
            PlayerData inviter = playerDataManager.GetPlayer(e.TeamInvitation.InviterId);
            PlayerData target = playerDataManager.GetPlayer(e.TeamInvitation.TargetId);

            ChatHelper.Say(inviter, $"Gracz {target.Name} przyjął twoje zaproszenie do drużyny");
        }

        private void TeamManager_OnPlayerInvited(object sender, Models.EventArgs.TeamInvitationEventArgs e)
        {
            PlayerData inviter = playerDataManager.GetPlayer(e.TeamInvitation.InviterId);
            PlayerData target = playerDataManager.GetPlayer(e.TeamInvitation.TargetId);

            ChatHelper.Say(target, $"Masz nowe zaproszenie od gracza {inviter.Name} do drużyny {e.Team.Name}");
        }
    }
}
