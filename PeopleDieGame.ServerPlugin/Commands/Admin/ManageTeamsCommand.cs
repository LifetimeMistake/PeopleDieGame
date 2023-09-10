using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using UnityEngine;
using SDG.Unturned;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class ManageTeamsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "teams";

        public string Help => "";

        public string Syntax => "<list/inspect/create/remove/getspawn/setspawn/resetspawn/getbase/setname/setdescription/setloadout/resetloadout/getbalance/setbalance/deposit/withdraw> <teamName/teamId> [<name/description/spawnpoint/loadoutName/loadoutId/amount>]";
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
                case "list":
                    VerbList(caller);
                    break;
                case "inspect":
                    VerbInspect(caller, verbArgs);
                    break;
                case "create":
                    VerbCreate(caller, verbArgs);
                    break;
                case "remove":
                    VerbRemove(caller, verbArgs);
                    break;
                case "getspawn":
                    VerbGetSpawn(caller, verbArgs);
                    break;
                case "setspawn":
                    VerbSetSpawn(caller, verbArgs);
                    break;
                case "resetspawn":
                    VerbResetSpawn(caller, verbArgs);
                    break;
                case "getbase":
                    VerbGetBase(caller, verbArgs);
                    break;
                case "setname":
                    VerbSetName(caller, verbArgs);
                    break;
                case "setdescription":
                    VerbSetDescription(caller, verbArgs);
                    break;
                case "setloadout":
                    VerbSetLoadout(caller, verbArgs);
                    break;
                case "resetloadout":
                    VerbResetLoadout(caller, verbArgs);
                    break;
                case "getbalance":
                    VerbGetBalance(caller, verbArgs);
                    break;
                case "setbalance":
                    VerbSetBalance(caller, verbArgs);
                    break;
                case "deposit":
                    VerbDeposit(caller, verbArgs);
                    break;
                case "withdraw":
                    VerbWithdraw(caller, verbArgs);
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

        private void VerbList(IRocketPlayer caller)
        {
            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();

                StringBuilder sb = new StringBuilder();
                ChatHelper.Say(caller, $"Lista drużyn:");
                foreach (Team team in teamManager.GetTeams().OrderBy(x => x.Id))
                {
                    sb.AppendLine($"ID: {team.Id} | Nazwa: {team.Name}");
                }
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o liście drużyn z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"ID: {team.Id}");
                sb.AppendLine($"Nazwa: \"{team.Name}\"");

                if (team.Description == "")
                {
                    sb.AppendLine("Opis: Brak");
                }
                else
                {
                    sb.AppendLine($"Opis: \"{team.Description}\"");
                }

                if (!team.DefaultLoadoutId.HasValue)
                {
                    sb.AppendLine("ID wyposażenia drużyny: Brak");
                }
                else
                {
                    sb.AppendLine($"ID wyposażenia drużyny: {team.DefaultLoadoutId}");
                }

                if (!team.LeaderId.HasValue)
                {
                    sb.AppendLine("Lider drużyny: Brak");
                }
                else
                {
                    sb.AppendLine($"Lider drużyny: {team.LeaderId.Value}");
                }

                VectorPAR? teamRespawnPoint = team.RespawnPoint;
                if (teamRespawnPoint == null)
                {
                    sb.AppendLine("Punkt odradzania: Brak");
                }
                else
                {
                    sb.AppendLine("Punkt odradzania:");
                    sb.AppendLine($"\t{teamRespawnPoint.Value}");
                }

                sb.AppendLine($"Stan konta: ${team.BankBalance}");

                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o drużynie z powodu błędu serwera: {ex.Message}");
            }

        }

        private void VerbCreate(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string name = string.Join(" ", command);
                if (teamManager.GetTeam(name) != null)
                {
                    ChatHelper.Say(caller, "Drużyna o tej nazwie już istnieje");
                    return;
                }

                Team team = teamManager.CreateTeam(name);
                ChatHelper.Say(caller, $"Utworzono drużynę o ID {team.Id}");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się utworzyć drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbRemove(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                if (!teamManager.DeleteTeam(team.Id))
                {
                    ChatHelper.Say(caller, "Nie udało się usunąć drużyny");
                    return;
                }

                ChatHelper.Say(caller, "Usunięto drużynę");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się usunąć drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                VectorPAR? teamRespawnPoint = team.RespawnPoint;
                if (teamRespawnPoint == null)
                {
                    ChatHelper.Say(caller, "Drużyna nie posiada respawn pointu");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Respawn point drużyny \"{searchTerm}\":");
                sb.AppendLine($"\t{teamRespawnPoint.Value.Position}");
                sb.AppendLine($"\t{teamRespawnPoint.Value.Rotation}");
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać punktu odradzania drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);
                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                if (!teamManager.HasClaim(team))
                {
                    ChatHelper.Say(caller, "Twoja drużyna nie posiada bazy");
                    return;
                }

                UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;

                VectorPAR? respawnPoint = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);
                if (!teamManager.IsInClaimRadius(team, respawnPoint.Value.Position))
                {
                    ChatHelper.Say(caller, "Znajdujesz się poza zasięgiem bazy");
                    return;
                }

                team.RespawnPoint = respawnPoint;
                ChatHelper.Say(caller, "Ustawiono punkt odradzania drużyny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić punktu odradzania drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbResetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                team.RespawnPoint = null;
                ChatHelper.Say(caller, "Zresetowano punkt odradzania drużyny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zresetować punktu odradzania drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGetBase(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                    return;
                }

                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);
                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                if (!teamManager.HasClaim(team))
                {
                    ChatHelper.Say(caller, "Drużyna nie posiada bazy");
                    return;
                }

                ClaimBubble claim = teamManager.GetClaim(team);
                Vector3S? basePoint = claim.origin;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Koordynaty bazy drużyny \"{team.Name}\":");
                sb.AppendLine($"\t{basePoint}");
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, $"Nie udało się pobrać informacji o bazie drużyny: {ex.Message}");
            }
        }

        private void VerbSetName(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                string name = string.Join(" ", command.Skip(1));
                teamManager.UpdateName(team, name);

                ChatHelper.Say(caller, $"Ustawiono nazwę drużyny na \"{name}\"");
            }
            catch (ArgumentException)
            {
                ChatHelper.Say(caller, "Nazwa drużyny nie może być pusta");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić nazwy drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetDescription(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                string desc = string.Join(" ", command.Skip(1));
                teamManager.UpdateDescription(team, desc);

                ChatHelper.Say(caller, $"Ustawiono opis drużyny na \"{desc}\"");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić opisu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetLoadout(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny i nazwę lub ID zestawu wyposażenia.");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();

                Team team = teamManager.ResolveTeam(command[0], false);
                if (team == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                Loadout loadout = loadoutManager.ResolveLoadout(command[1], false);
                if (loadout == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono zestawu wyposażenia");
                    return;
                }

                teamManager.UpdateLoadout(team, loadout);
                ChatHelper.Say(caller, "Ustawiono nowy zestaw wyposażenia!");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zmienić domyślnego zestawu wyposażenia z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbResetLoadout(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                teamManager.UpdateLoadout(team, null);
                ChatHelper.Say(caller, "Zresetowano zestaw wyposażenia drużyny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zresetować zestawu wyposażenia drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGetBalance(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                double amount = team.BankBalance;
                ChatHelper.Say(caller, $"Drużyna \"{team.Name}\" ma ${amount} w banku");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać ilości środków drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbSetBalance(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                if (!float.TryParse(command[1], out float amount))
                {
                    ChatHelper.Say(caller, "Musisz podać ilość środków do ustawienia");
                    return;
                }

                teamManager.UpdateBalance(team, amount);
                ChatHelper.Say(caller, "Ustawiono ilość środków drużyny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić ilości środków drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbDeposit(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                if (!float.TryParse(command[1], out float amount))
                {
                    ChatHelper.Say(caller, "Musisz podać ilość środków do zdeponowania");
                    return;
                }

                teamManager.AddBalance(team, amount);
                ChatHelper.Say(caller, "Zdeponowano środki do banku drużyny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zdeponować środków do banku drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbWithdraw(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                if (!float.TryParse(command[1], out float amount))
                {
                    ChatHelper.Say(caller, "Musisz podać ilość środków do zdeponowania");
                    return;
                }

                teamManager.RemoveBalance(team, amount);
                ChatHelper.Say(caller, "Wypłacono środki z banku drużyny");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się wypłacić środków z banku drużyny z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
