using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.Teams
{
    public class InvitesCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "invites";

        public string Help => "Pokazuje listę oczekujących zaproszeń do drużyn.";

        public string Syntax => "<team name/team id>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można wyświetlać listy zaproszeń do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Już należysz do drużyny!");
                    return;
                }

                Dictionary<Team, TeamInvitation> teamInvitations = new Dictionary<Team, TeamInvitation>();
                foreach (Team team in teamManager.GetTeams())
                {
                    TeamInvitation teamInvitation = team.GetInvitations().FirstOrDefault(x => x.TargetId == callerPlayerData.Id);
                    if (teamInvitation != null)
                        teamInvitations.Add(team, teamInvitation);
                }

                if (teamInvitations.Count == 0)
                {
                    ChatHelper.Say(caller, "Nie posiadasz żadnych oczekujących zaproszeń.");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                ChatHelper.Say(caller, "Twoje oczekujące zaproszenia:");
                foreach (KeyValuePair<Team, TeamInvitation> kvp in teamInvitations)
                {
                    sb.AppendLine($"Zaproszenie do \"{kvp.Key.Name}\", wysłano {(DateTime.Now - kvp.Value.InviteDate).TotalSeconds}s temu, wygasa za {kvp.Value.GetTimeRemaining().TotalSeconds}s");
                }
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się wyświetlić listy zaproszeń z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
