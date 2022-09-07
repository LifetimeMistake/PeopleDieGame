using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManageTeamsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "teams";

        public string Help => "";

        public string Syntax => "<list/inspect/create/remove/getspawn/setspawn/resetspawn/setname/setdescription/setloadout/resetloadout/getbalance/setbalance/deposit/withdraw> <teamName/teamId> [<name/description/spawnpoint/loadoutName/loadoutId/amount>]";
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
                case "list":
                    VerbList(caller, verbArgs);
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
                    UnturnedChat.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            UnturnedChat.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbList(IRocketPlayer caller, string[] command)
        {
            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();

                UnturnedChat.Say(caller, $"Lista drużyn:");
                foreach (Team team in teamManager.GetTeams().OrderBy(x => x.Id))
                {
                    UnturnedChat.Say(caller, $"ID: {team.Id} | Nazwa: {team.Name}");
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać informacji o liście drużyn z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbInspect(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                UnturnedChat.Say(caller, $"ID: {team.Id}");
                UnturnedChat.Say(caller, $"Nazwa: \"{team.Name}\"");

                if (team.Description == "")
                {
                    UnturnedChat.Say(caller, "Opis: Brak");
                }
                else
                {
                    UnturnedChat.Say(caller, $"Opis: \"{team.Description}\"");
                }

                if (!team.DefaultLoadoutId.HasValue)
                {
                    UnturnedChat.Say(caller, "ID wyposażenia drużyny: Brak");
                }
                else
                {
                    UnturnedChat.Say(caller, $"ID wyposażenia drużyny: {team.DefaultLoadoutId}");
                }

                if (!team.LeaderId.HasValue)
                {
                    UnturnedChat.Say(caller, "Lider drużyny: Brak");
                }
                else
                {
                    UnturnedChat.Say(caller, $"Lider drużyny: {team.LeaderId.Value}");
                }

                VectorPAR? teamRespawnPoint = team.RespawnPoint;
                if (teamRespawnPoint == null)
                {
                    UnturnedChat.Say(caller, "Punkt odradzania: Brak");
                }
                else
                {
                    UnturnedChat.Say(caller, "Punkt odradzania:");
                    UnturnedChat.Say(caller, $"\t{teamRespawnPoint.Value.Position}");
                    UnturnedChat.Say(caller, $"\t{teamRespawnPoint.Value.Rotation}");
                }

                UnturnedChat.Say(caller, $"Stan konta: ${team.BankBalance}");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać informacji o drużynie z powodu błędu serwera: {ex.Message}");
            }

        }

        private void VerbCreate(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string name = string.Join(" ", command);
                if (teamManager.GetTeamByName(name) != null)
                {
                    UnturnedChat.Say(caller, "Drużyna o tej nazwie już istnieje");
                    return;
                }

                Team team = teamManager.CreateTeam(name);
                UnturnedChat.Say(caller, $"Utworzono drużynę o ID {team.Id}");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się utworzyć drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbRemove(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                if (!teamManager.DeleteTeam(team.Id))
                {
                    UnturnedChat.Say(caller, "Nie udało się usunąć drużyny");
                    return;
                }

                UnturnedChat.Say(caller, "Usunięto drużynę");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się usunąć drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                VectorPAR? teamRespawnPoint = team.RespawnPoint;
                if (teamRespawnPoint == null)
                {
                    UnturnedChat.Say(caller, "Drużyna nie posiada respawn pointu");
                    return;
                }

                UnturnedChat.Say(caller, $"Respawn point drużyny \"{searchTerm}\":");
                UnturnedChat.Say(caller, $"\t{teamRespawnPoint.Value.Position}");
                UnturnedChat.Say(caller, $"\t{teamRespawnPoint.Value.Rotation}");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać punktu odradzania drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                VectorPAR? respawnPoint = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);
                team.RespawnPoint = respawnPoint;
                UnturnedChat.Say(caller, "Ustawiono respawn point drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić punktu odradzania drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbResetSpawn(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                team.RespawnPoint = null;
                UnturnedChat.Say(caller, "Zresetowano punkt odradzania drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zresetować punktu odradzania drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetName(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                string name = string.Join(" ", command.Skip(1));
                team.SetName(name);

                UnturnedChat.Say(caller, $"Ustawiono nazwę drużyny na \"{name}\"");
            }
            catch (ArgumentException)
            {
                UnturnedChat.Say(caller, "Nazwa drużyny nie może być pusta");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić nazwy drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetDescription(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                string desc = string.Join(" ", command.Skip(1));
                team.SetDescription(desc);

                UnturnedChat.Say(caller, $"Ustawiono opis drużyny na \"{desc}\"");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić opisu z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetLoadout(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny i nazwę lub ID zestawu wyposażenia.");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();

                Team team = teamManager.ResolveTeam(command[0], false);
                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                Loadout loadout = loadoutManager.ResolveLoadout(command[1], false);
                if (loadout == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono zestawu wyposażenia");
                    return;
                }

                team.SetDefaultLoadout(loadout);
                UnturnedChat.Say(caller, "Ustawiono nowy zestaw wyposażenia!");
            }
            catch(Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zmienić domyślnego zestawu wyposażenia z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbResetLoadout(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                string searchTerm = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(searchTerm, false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{searchTerm}\"");
                    return;
                }

                team.SetDefaultLoadout(null);
                UnturnedChat.Say(caller, "Zresetowano zestaw wyposażenia drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zresetować zestawu wyposażenia drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGetBalance(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                double amount = teamManager.GetBankBalance(team);
                UnturnedChat.Say(caller, $"Drużyna \"{team.Name}\" ma ${amount} w banku");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać ilości środków drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbSetBalance(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    UnturnedChat.Say(caller, "Musisz podać ilość środków do ustawienia");
                    return;
                }

                teamManager.SetBankBalance(team, amount);
                UnturnedChat.Say(caller, "Ustawiono ilość środków drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić ilości środków drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbDeposit(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    UnturnedChat.Say(caller, "Musisz podać ilość środków do zdeponowania");
                    return;
                }

                teamManager.DepositIntoBank(team, amount);
                UnturnedChat.Say(caller, "Zdeponowano środki do banku drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zdeponować środków do banku drużyny z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbWithdraw(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team = teamManager.ResolveTeam(command[0], false);

                if (team == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono drużyny");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    UnturnedChat.Say(caller, "Musisz podać ilość środków do zdeponowania");
                    return;
                }

                teamManager.WithdrawFromBank(team, amount);
                UnturnedChat.Say(caller, "Wypłacono środki z banku drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się wypłacić środków z banku drużyny z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
