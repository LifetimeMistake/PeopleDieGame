using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

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
                    UnturnedChat.Say(caller, "Nie można wyświetlać listy zaproszeń do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Już należysz do drużyny!");
                    return;
                }

                Dictionary<Team, TeamInvitation> teamInvitations = new Dictionary<Team, TeamInvitation>();
                foreach (Team team in teamManager.GetTeams())
                    teamInvitations.Add(team, team.GetInvitations().FirstOrDefault(x => x.TargetId == callerPlayerData.Id));

                if(teamInvitations.Count == 0)
                {
                    UnturnedChat.Say(caller, "Nie posiadasz żadnych oczekujących zaproszeń.");
                    return;
                }

                UnturnedChat.Say(caller, "Twoje oczekujące zaproszenia:");
                foreach (KeyValuePair<Team, TeamInvitation> kvp in teamInvitations)
                {
                    UnturnedChat.Say(caller, $"Zaproszenie do \"{kvp.Key.Name}\", wysłano {(DateTime.Now - kvp.Value.InviteDate).TotalSeconds}s temu, wygasa za {kvp.Value.GetTimeRemaining().TotalSeconds}s");
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zaakceptować zaproszenia z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
