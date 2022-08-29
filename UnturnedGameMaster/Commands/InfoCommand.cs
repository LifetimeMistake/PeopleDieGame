using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Serialisation;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands
{
    public class InfoCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "info";

        public string Help => "Wyświetla informacje o graczach, drużynach i innych obiektach!";

        public string Syntax => "<player/team/game> [<name/id>]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, $"Musisz podać argument.");
                ShowSyntax(caller);
                return;
            }

            string[] verbArgs = command.Skip(1).ToArray();
            switch(command[0].ToLowerInvariant())
            {
                case "player":
                    VerbPlayerInfo(caller, verbArgs);
                    break;
                case "team":
                    VerbTeamInfo(caller, verbArgs);
                    break;
                case "game":
                    VerbGameInfo(caller);
                    break;
                default:
                    UnturnedChat.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            UnturnedChat.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbPlayerInfo(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData;

                if (command.Length == 0)
                {
                    playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                    if (playerData == null)
                    {
                        UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć akt gracza??)");
                        return;
                    }
                }
                else
                {
                    string searchTerm = string.Join(" ", command);
                    playerData = playerDataManager.ResolvePlayer(searchTerm, false);
                    if (playerData == null)
                    {
                        UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{searchTerm}\"");
                        return;
                    }
                }

                UnturnedChat.Say(caller, playerDataManager.GetPlayerSummary(playerData));
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się odnaleźć danych gracza z powodu błedu serwera: {ex.Message}");
                return;
            }
        }

        private void VerbTeamInfo(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team;

                if (command.Length == 0)
                {
                    PlayerData playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                    if (playerData == null)
                    {
                        UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć akt gracza??)");
                        return;
                    }

                    if (!playerData.TeamId.HasValue)
                    {
                        UnturnedChat.Say(caller, "Nie należysz do żadnej z drużyn.");
                        return;
                    }

                    team = teamManager.GetTeam(playerData.TeamId.Value);
                }
                else
                {
                    string searchTerm = string.Join(" ", command);
                    team = teamManager.ResolveTeam(searchTerm, false);
                }

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny");
                    return;
                }

                UnturnedChat.Say(caller, teamManager.GetTeamSummary(team));
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się odczytać danych drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGameInfo(IRocketPlayer caller)
        {
            try
            {
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
                UnturnedChat.Say(caller, gameManager.GetGameSummary());
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się odczytać stanu gry z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
