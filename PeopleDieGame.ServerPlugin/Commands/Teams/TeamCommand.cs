using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using SDG.Unturned;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Commands.Teams
{
    public class TeamCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "team";

        public string Help => "Polecenie służące do zarządzania Twoją drużyną.";

        public string Syntax => "<create/disband/invite/cancelInvite/leave/kick/promote/name/description/loadout> [<team name/player name/new team name/new team description/new loadout name>]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Musisz podać argument.");
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
                case "leave":
                    VerbLeave(caller);
                    break;
                case "setspawn":
                    VerbSetSpawn(caller);
                    break;
                default:
                    ChatHelper.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbCreate(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę drużyny.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można tworzyć nowych drużyn po rozpoczęciu gry!");
                    return;
                }

                UnturnedPlayer player = (UnturnedPlayer)caller;
                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
                if (playerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie udało się odnaleźć profilu gracza??)");
                    return;
                }

                if (playerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Już należysz do drużyny! Użyj polecenia /leaveteam lub /disbandteam, aby ją opuścić");
                    return;
                }

                string teamName = string.Join(" ", command);
                if (teamManager.GetTeamByName(teamName, true) != null)
                {
                    ChatHelper.Say(caller, "Drużyna o tej nazwie już istnieje.");
                    return;
                }

                Team team = teamManager.CreateTeam(teamName);
                if (!teamManager.JoinTeam(playerData, team))
                {
                    ChatHelper.Say(caller, "Warn: Nie udało się dołączyć do utworzonej drużyny z powodu błędu systemu, poproś administratora o pomoc.");
                }

                ChatHelper.Say(caller, $"Utworzono drużynę \"{teamName}\"! Zaproś nowych graczy przy użyciu polecenia /team invite <nazwa gracza>");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się utworzyć drużyny z powodu błedu serwera: {ex.Message}");
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
                    ChatHelper.Say(caller, "Nie można rozwiązywać drużyn po rozpoczęciu gry!");
                    return;
                }

                UnturnedPlayer player = (UnturnedPlayer)caller;
                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
                if (playerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie udało się odnaleźć profilu gracza??)");
                    return;
                }

                if (!playerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do drużyny!");
                    return;
                }

                Team team = teamManager.GetTeam(playerData.TeamId.Value);

                if (!teamManager.DeleteTeam(team.Id))
                {
                    ChatHelper.Say(caller, "Nie udało się rozwiązać drużyny z powodu błędu systemu, poproś administratora o pomoc.");
                    return;
                }

                ChatHelper.Say(caller, $"Rozwiązano drużynę!");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się utworzyć drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbSetSpawn(IRocketPlayer caller)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                UnturnedPlayer player = (UnturnedPlayer)caller;
                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)player.CSteamID);

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do drużyny!");
                    return;
                }

                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);

                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Nie jesteś liderem drużyny");
                }

                if (!teamManager.HasClaim(team))
                {
                    ChatHelper.Say(caller, "Twoja drużyna nie posiada bazy");
                    return;
                }

                VectorPAR? respawnPoint = new VectorPAR(player.Position, (byte)player.Rotation);
                if (!teamManager.IsInClaimRadius(team, respawnPoint.Value.Position))
                {
                    ChatHelper.Say(caller, "Znajdujesz się poza zasięgiem bazy");
                    return;
                }

                team.RespawnPoint = respawnPoint;
                ChatHelper.Say(caller, "Ustawiono respawn point drużyny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić punktu odradzania drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbInvite(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można zapraszać graczy do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Tylko lider drużyny może zapraszać innych graczy!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (callerPlayerData == targetPlayerData)
                {
                    ChatHelper.Say(caller, $"Nie możesz zaprosić samego siebie lmao");
                    return;
                }

                if (targetPlayerData.TeamId != null)
                {
                    if (targetPlayerData.TeamId == team.Id)
                        ChatHelper.Say(caller, $"Gracz należy już do Twojej drużyny");
                    else
                        ChatHelper.Say(caller, $"Gracz należy już do innej drużyny");

                    return;
                }

                if (team.GetInvitations().Any(x => x.TargetId == targetPlayerData.Id))
                {
                    ChatHelper.Say(caller, "Gracz posiada już oczekujące zaproszenie do tej drużyny.");
                    return;
                }

                if (!teamManager.InvitePlayer(team, targetPlayerData))
                {
                    ChatHelper.Say(caller, "Nie udało się zaprosić gracza do Twojej drużyny z powodu błedu serwera");
                    return;
                }

                ChatHelper.Say(caller, $"Wysłano zaproszenie do \"{targetPlayerData.Name}\"!");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zaprosić gracza do drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbCancelInvite(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można zapraszać graczy do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Tylko lider drużyny może anulować zaproszenia do drużyny!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (!team.GetInvitations().Any(x => x.TargetId == targetPlayerData.Id))
                {
                    ChatHelper.Say(caller, "Ten gracz nie ma oczekującego zaproszenia do Twojej drużyny.");
                    return;
                }

                if (!teamManager.CancelInvitation(team, targetPlayerData))
                {
                    ChatHelper.Say(caller, "Nie udało się anulować zaproszenia z powodu błedu serwera");
                    return;
                }

                ChatHelper.Say(caller, $"Anulowano zaproszenie.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zaprosić gracza do drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbKick(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można wyrzucać graczy z drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Tylko lider drużyny może wyrzucać graczy z drużyny!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (targetPlayerData.TeamId != team.Id)
                {
                    ChatHelper.Say(caller, "Gracz nie znajduje się w Twojej drużynie.");
                    return;
                }

                if (!teamManager.LeaveTeam(targetPlayerData))
                {
                    ChatHelper.Say(caller, "Nie udało się wyrzucić gracza z powodu błedu serwera.");
                    return;
                }

                ChatHelper.Say(caller, $"Gracz \"{targetPlayerData.Name}\" został wyrzucony z drużyny.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zaprosić gracza do drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbPromote(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę gracza.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Tylko lider drużyny może przekazać rolę przywódcy innym!");
                    return;
                }

                string playerName = string.Join(" ", command);
                PlayerData targetPlayerData = playerDataManager.ResolvePlayer(playerName, false);
                if (targetPlayerData == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono gracza \"{playerName}\"");
                    return;
                }

                if (targetPlayerData.TeamId != team.Id)
                {
                    ChatHelper.Say(caller, "Gracz nie znajduje się w Twojej drużynie.");
                    return;
                }

                if (!teamManager.SetLeader(team, targetPlayerData))
                {
                    ChatHelper.Say(caller, "Nie udało się zmienić przywódcy z powodu błedu serwera.");
                    return;
                }

                ChatHelper.Say(caller, $"Gracz \"{targetPlayerData.Name}\" został awansowany do lidera drużyny.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się awansować gracza do drużyny z powodu błedu serwera: {ex.Message}");
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
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Tylko lider może zmieniać nazwę drużyny!");
                    return;
                }

                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, $"Nazwa Twojej drużyny: \"{team.Name}\"");
                }
                else
                {
                    if (gameManager.GetGameState() != Enums.GameState.InLobby)
                    {
                        ChatHelper.Say(caller, "Nie można zmieniać nazw drużyn po rozpoczęciu gry!");
                        return;
                    }

                    string teamName = string.Join(" ", command);
                    team.SetName(teamName);
                    ChatHelper.Say(caller, $"Ustawiono nazwę Twojej drużyny na: \"{teamName}\"");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się wykonać polecenia z powodu błedu serwera: {ex.Message}");
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
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Tylko lider może zmieniać opis drużyny!");
                    return;
                }

                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, $"Opis Twojej drużyny: \"{team.Description}\"");
                }
                else
                {
                    if (gameManager.GetGameState() != Enums.GameState.InLobby)
                    {
                        ChatHelper.Say(caller, "Nie można zmieniać opisów drużyn po rozpoczęciu gry!");
                        return;
                    }

                    string teamDescription = string.Join(" ", command);
                    team.SetDescription(teamDescription);
                    ChatHelper.Say(caller, $"Ustawiono opis Twojej drużyny na: \"{teamDescription}\"");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się wykonać polecenia z powodu błedu serwera: {ex.Message}");
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
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (!callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny.");
                    return;
                }

                Team team = teamManager.GetTeam(callerPlayerData.TeamId.Value);
                if (team.LeaderId != callerPlayerData.Id)
                {
                    ChatHelper.Say(caller, "Tylko lider może zmieniać zestaw wyposażenia drużyny!");
                    return;
                }

                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, $"Zestaw Twojej drużyny: {(team.DefaultLoadoutId.HasValue ? $"\"{loadoutManager.GetLoadout(team.DefaultLoadoutId.Value).Name}\"" : "Brak")}");
                    return;
                }
                else
                {
                    if (gameManager.GetGameState() != Enums.GameState.InLobby)
                    {
                        ChatHelper.Say(caller, "Nie można zmieniać zestawów wyposażeń drużyn po rozpoczęciu gry!");
                        return;
                    }

                    string loadoutName = string.Join(" ", command);
                    Loadout loadout = loadoutManager.ResolveLoadout(loadoutName, false);
                    if (loadout == null)
                    {
                        ChatHelper.Say(caller, $"Nie udało się znaleźć zestawu wyposażenia o nazwie \"{loadoutName}\"");
                        return;
                    }

                    team.SetDefaultLoadout(loadout);
                    ChatHelper.Say(caller, $"Ustawiono zestaw Twojej drużyny na: \"{loadout.Name}\"");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się wykonać polecenia z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbLeave(IRocketPlayer caller)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można opuszczać drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (playerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie udało się odnaleźć profilu gracza??)");
                    return;
                }

                if (!playerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Nie należysz do żadnej drużyny!");
                    return;
                }

                if (!teamManager.LeaveTeam(playerData))
                {
                    ChatHelper.Say(caller, "Nie udało się opuścić drużyny z powodu błedu systemu");
                    return;
                }

                ChatHelper.Say(caller, "Opuszczono drużynę.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, $"Nie udało się opuścić drużyny z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
