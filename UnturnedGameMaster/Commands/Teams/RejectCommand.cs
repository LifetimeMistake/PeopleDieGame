using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.Teams
{
    public class RejectCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "reject";

        public string Help => "Odrzuca oczekujące zaproszenie do drużyny.";

        public string Syntax => "<team name/team id>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę drużyny której zaproszenie chcesz odrzucić.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można odrzucać zaproszeń do drużyn po rozpoczęciu gry!");
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

                string teamName = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(teamName, false);
                if (team == null)
                {
                    ChatHelper.Say(caller, "Taka drużyna nie istnieje!");
                    return;
                }

                if (!team.GetInvitations().Any(x => x.TargetId == callerPlayerData.Id))
                {
                    ChatHelper.Say(caller, "Nie posiadasz oczekującego zaproszenia od tej drużyny.");
                    return;
                }

                if (!teamManager.RejectInvitation(team, callerPlayerData))
                {
                    ChatHelper.Say(caller, "Nie udało się odrzucić zaproszenia z powodu błedu systemu.");
                    return;
                }

                ChatHelper.Say(caller, "Odrzucono zaproszenie.");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się odrzucić zaproszenia z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
