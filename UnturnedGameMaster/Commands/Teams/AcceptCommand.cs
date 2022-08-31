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
using static Rocket.Unturned.Events.UnturnedPlayerEvents;

namespace UnturnedGameMaster.Commands.Teams
{
    public class AcceptCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "accept";

        public string Help => "Akceptuje oczekujące zaproszenia do drużyny.";

        public string Syntax => "<team name/team id>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say("Musisz podać nazwę drużyny której zaproszenie chcesz przyjąć.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    UnturnedChat.Say("Nie można przyjmować zaproszeń do drużyn po rozpoczęciu gry!");
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

                string teamName = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(teamName, false);
                if (team == null)
                {
                    UnturnedChat.Say(caller, "Taka drużyna nie istnieje!");
                    return;
                }

                if (!team.GetInvitations().Any(x => x.TargetId == callerPlayerData.Id))
                {
                    UnturnedChat.Say(caller, "Nie posiadasz oczekującego zaproszenia od tej drużyny.");
                    return;
                }

                if (!teamManager.AcceptInvitation(team, callerPlayerData))
                {
                    UnturnedChat.Say(caller, "Nie udało się zaakceptować zaproszenia z powodu błedu systemu.");
                    return;
                }

                UnturnedChat.Say("Zaakceptowano zaproszenie! Witaj na pokładzie!");
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zaakceptować zaproszenia z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
