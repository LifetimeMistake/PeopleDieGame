using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
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
    public class TeamCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "team";

        public string Help => "Polecenie służące do zarządzania Twoją drużyną.";

        public string Syntax => "<create/disband/invite/cancelInvite/kick/promote/name/description/loadout> [<team name/player name/new team name/new team description/new loadout name>]";

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
            switch (command[0].ToLowerInvariant())
            {
                case "create":
                    VerbCreate(caller, verbArgs);
                    break;
                case "disband":
                    VerbDisband(caller);
                    break;
                case "invite":
                    VerbInvite(caller, verbArgs);
                    break;
                case "cancelinvite":
                    VerbCancelInvite(caller, verbArgs);
                    break;
                case "kick":
                    VerbKick(caller, verbArgs);
                    break;
                case "promote":
                    VerbPromote(caller, verbArgs);
                    break;
                case "name":
                    VerbName(caller, verbArgs);
                    break;
                case "description":
                    VerbDescription(caller, verbArgs);
                    break;
                case "loadout":
                    VerbLoadout(caller, verbArgs);
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

        private void VerbCreate(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę drużyny.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    UnturnedChat.Say(caller, "Nie można tworzyć nowych drużyn po rozpoczęciu gry!");
                    return;
                }

                UnturnedPlayer player = (UnturnedPlayer)caller;
                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
                if (playerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie udało się odnaleźć profilu gracza??)");
                    return;
                }

                if (playerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Już należysz do drużyny! Użyj polecenia /leaveteam lub /disbandteam, aby ją opuścić");
                    return;
                }

                string teamName = string.Join(" ", command);
                if (teamManager.GetTeamByName(teamName, true) != null)
                {
                    UnturnedChat.Say(caller, "Drużyna o tej nazwie już istnieje.");
                    return;
                }

                Team team = teamManager.CreateTeam(teamName);
                if (!teamManager.JoinTeam(playerData, team))
                {
                    UnturnedChat.Say(caller, "Warn: Nie udało się dołączyć do utworzonej drużyny z powodu błędu systemu, poproś administratora o pomoc.");
                }

                if (!teamManager.SetLeader(team, playerData))
                {
                    UnturnedChat.Say(caller, "Warn: Nie udało się mianować Cię dowódcą drużyny z powodu błędu systemu, poproś administratora o pomoc.");
                }

                UnturnedChat.Say(caller, $"Utworzono drużynę \"{teamName}\"! Zaproś nowych graczy przy użyciu polecenia /invite <nazwa gracza>");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się utworzyć drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbDisband(IRocketPlayer caller)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    UnturnedChat.Say("Nie można rozwiązywać drużyn po rozpoczęciu gry!");
                    return;
                }

                UnturnedPlayer player = (UnturnedPlayer)caller;
                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
                if (playerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie udało się odnaleźć profilu gracza??)");
                    return;
                }

                if (!playerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do drużyny!");
                    return;
                }

                Team team = teamManager.GetTeam(playerData.TeamId.Value);

                if (!teamManager.DeleteTeam(team.Id))
                {
                    UnturnedChat.Say(caller, "Nie udało się rozwiązać drużyny z powodu błędu systemu, poproś administratora o pomoc.");
                    return;
                }

                UnturnedChat.Say(caller, $"Rozwiązano drużynę!");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się utworzyć drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbInvite(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    UnturnedChat.Say("Nie można zapraszać graczy do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    UnturnedChat.Say(caller, "Tylko lider drużyny może zapraszać innych graczy!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    UnturnedChat.Say($"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (callerPlayerData == targetPlayerData)
                {
                    UnturnedChat.Say($"Nie możesz zaprosić samego siebie lmao");
                    return;
                }

                if (targetPlayerData.TeamId != null)
                {
                    if (targetPlayerData.TeamId == team.Id)
                        UnturnedChat.Say($"Gracz należy już do Twojej drużyny");
                    else
                        UnturnedChat.Say($"Gracz należy już do innej drużyny");
                    
                    return;
                }

                if (team.GetInvitations().Any(x => x.TargetId == targetPlayerData.Id))
                {
                    UnturnedChat.Say(caller, "Gracz posiada już oczekujące zaproszenie do tej drużyny.");
                    return;
                }

                if (!teamManager.InvitePlayer(team, targetPlayerData))
                {
                    UnturnedChat.Say(caller, "Nie udało się zaprosić gracza do Twojej drużyny z powodu błedu serwera");
                    return;
                }

                UnturnedChat.Say($"Wysłano zaproszenie do \"{targetPlayerData.Name}\"!");
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zaprosić gracza do drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbCancelInvite(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    UnturnedChat.Say("Nie można zapraszać graczy do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    UnturnedChat.Say(caller, "Tylko lider drużyny może anulować zaproszenia do drużyny!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    UnturnedChat.Say($"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (!team.GetInvitations().Any(x => x.TargetId == targetPlayerData.Id))
                {
                    UnturnedChat.Say(caller, "Ten gracz nie ma oczekującego zaproszenia do Twojej drużyny.");
                    return;
                }

                if (!teamManager.CancelInvitation(team, targetPlayerData))
                {
                    UnturnedChat.Say(caller, "Nie udało się anulować zaproszenia z powodu błedu serwera");
                    return;
                }

                UnturnedChat.Say($"Anulowano zaproszenie.");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zaprosić gracza do drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbKick(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    UnturnedChat.Say("Nie można wyrzucać graczy z drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    UnturnedChat.Say(caller, "Tylko lider drużyny może wyrzucać graczy z drużyny!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    UnturnedChat.Say($"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (targetPlayerData.TeamId != team.Id)
                {
                    UnturnedChat.Say(caller, "Gracz nie znajduje się w Twojej drużynie.");
                    return;
                }

                if (!teamManager.LeaveTeam(targetPlayerData))
                {
                    UnturnedChat.Say(caller, "Nie udało się wyrzucić gracza z powodu błedu serwera.");
                    return;
                }

                UnturnedChat.Say($"Gracz \"{targetPlayerData.Name}\" został wyrzucony z drużyny.");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zaprosić gracza do drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbPromote(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    UnturnedChat.Say(caller, "Tylko lider drużyny może przekazać rolę przywódcy innym!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    UnturnedChat.Say($"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (targetPlayerData.TeamId != team.Id)
                {
                    UnturnedChat.Say(caller, "Gracz nie znajduje się w Twojej drużynie.");
                    return;
                }

                if (!teamManager.SetLeader(team, targetPlayerData))
                {
                    UnturnedChat.Say(caller, "Nie udało się zmienić przywódcy z powodu błedu serwera.");
                    return;
                }

                UnturnedChat.Say($"Gracz \"{targetPlayerData.Name}\" został awansowany do lidera drużyny.");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się awansować gracza do drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbName(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    UnturnedChat.Say(caller, "Tylko lider może zmieniać nazwę drużyny!");
                    return;
                }

                if (command.Length == 0)
                {
                    UnturnedChat.Say(caller, $"Nazwa Twojej drużyny: \"{team.Name}\"");
                }
                else
                {
                    if (gameManager.GetGameState() != Enums.GameState.InLobby)
                    {
                        UnturnedChat.Say("Nie można zmieniać nazw drużyn po rozpoczęciu gry!");
                        return;
                    }

                    string teamName = string.Join(" ", command);
                    team.SetName(teamName);
                    UnturnedChat.Say(caller, $"Ustawiono nazwę Twojej drużyny na: \"{teamName}\"");
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się wykonać polecenia z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbDescription(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    UnturnedChat.Say(caller, "Tylko lider może zmieniać opis drużyny!");
                    return;
                }

                if (command.Length == 0)
                {
                    UnturnedChat.Say(caller, $"Opis Twojej drużyny: \"{team.Description}\"");
                }
                else
                {
                    if (gameManager.GetGameState() != Enums.GameState.InLobby)
                    {
                        UnturnedChat.Say("Nie można zmieniać opisów drużyn po rozpoczęciu gry!");
                        return;
                    }

                    string teamDescription = string.Join(" ", command);
                    team.SetDescription(teamDescription);
                    UnturnedChat.Say(caller, $"Ustawiono opis Twojej drużyny na: \"{teamDescription}\"");
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się wykonać polecenia z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbLoadout(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    UnturnedChat.Say(caller, "Tylko lider może zmieniać zestaw wyposażenia drużyny!");
                    return;
                }

                if (command.Length == 0)
                {
                    UnturnedChat.Say(caller, $"Zestaw Twojej drużyny: {(team.DefaultLoadoutId.HasValue ? $"\"{loadoutManager.GetLoadout(team.DefaultLoadoutId.Value).Name}\"" : "Brak")}");
                    return;
                }
                else
                {
                    if (gameManager.GetGameState() != Enums.GameState.InLobby)
                    {
                        UnturnedChat.Say("Nie można zmieniać zestawów wyposażeń drużyn po rozpoczęciu gry!");
                        return;
                    }

                    string loadoutName = string.Join(" ", command);
                    Loadout loadout = loadoutManager.ResolveLoadout(loadoutName, false);
                    if (loadout == null)
                    {
                        UnturnedChat.Say($"Nie udało się znaleźć zestawu wyposażenia o nazwie \"{loadoutName}\"");
                        return;
                    }

                    team.SetDefaultLoadout(loadout);
                    UnturnedChat.Say(caller, $"Ustawiono zestaw Twojej drużyny na: \"{loadout.Name}\"");
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się wykonać polecenia z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
