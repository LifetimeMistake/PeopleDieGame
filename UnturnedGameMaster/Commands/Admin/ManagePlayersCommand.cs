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
    public class ManagePlayersCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "players";

        public string Help => "";

        public string Syntax => "<getteam/jointeam/leaveteam/promoteplayer/setbio/getbalance/setbalance/deposit/withdraw> <playerName/playerId> [<teamName/teamId/bio/amount>]";
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
                case "getteam":
                    VerbGetTeam(caller, verbArgs);
                    break;
                case "jointeam":
                    VerbJoinTeam(caller, verbArgs);
                    break;
                case "leaveteam":
                    VerbLeaveTeam(caller, verbArgs);
                    break;
                case "promoteplayer":
                    VerbPromotePlayer(caller, verbArgs);
                    break;
                case "setbio":
                    VerbSetBio(caller, verbArgs);
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

        private void VerbGetTeam(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                PlayerData playerData;
                string searchTerm = string.Join(" ", command);

                playerData = playerDataManager.ResolvePlayer(searchTerm, false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{searchTerm}\"");
                    return;
                }
                else
                {
                    if (playerData.TeamId.HasValue)
                    {
                        Team team = teamManager.GetTeam(playerData.TeamId.Value);
                        UnturnedChat.Say(caller, $"Gracz należy do drużyny: \"{team.Name}\", ID: {team.Id}");
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Gracz nie należy do żadnej drużyny");
                    }
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać drużyny gracza z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbJoinTeam(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza oraz nazwę lub ID drużyny");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);
                Team team = teamManager.ResolveTeam(command[1], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\"");
                    return;
                }

                if (playerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Gracz już jest w drużynie");
                    return;
                }

                if (team == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono drużyny \"{command[1]}\"");
                    return;
                }

                if (!teamManager.JoinTeam(playerData, team))
                {
                    UnturnedChat.Say(caller, "Nie udało się dodać gracza do drużyny");
                    return;
                }

                UnturnedChat.Say(caller, "Dodano gracza do drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się dodać gracza do drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbLeaveTeam(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\".");
                    return;
                }

                if (!playerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Gracz nie jest w drużynie.");
                    return;
                }

                if (!teamManager.LeaveTeam(playerData))
                {
                    UnturnedChat.Say(caller, "Nie udało się usunąć gracza z drużyny.");
                    return;
                }

                UnturnedPlayer player = UnturnedPlayer.FromCSteamID((CSteamID)playerData.Id);
                if (player != null)
                {
                    UnturnedChat.Say(player, $"Zostałeś wyrzucony z drużyny.");
                }

                UnturnedChat.Say(caller, "Usunięto gracza z drużyny.");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się usunąć gracza z drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbPromotePlayer(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);
                Team team = teamManager.ResolveTeam(command[1], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\"");
                    return;
                }

                if (!playerData.TeamId.HasValue)
                {
                    UnturnedChat.Say(caller, "Gracz nie jest w drużynie");
                    return;
                }

                if (playerData.Id == team.LeaderId)
                {
                    UnturnedChat.Say(caller, "Gracz jest już liderem drużyny");
                    return;
                }

                team.SetTeamLeader(playerData);
                UnturnedChat.Say(caller, "Awansowano gracza na lidera drużyny");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się awansować gracza z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBio(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\"");
                    return;
                }

                string bio = string.Join(" ", command.Skip(1));
                playerData.SetBio(bio);

                if (bio == "")
                {
                    UnturnedChat.Say(caller, "Zresetowano bio gracza");
                    return;
                }

                UnturnedChat.Say(caller, $"Ustawiono bio gracza na \"{bio}\"");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić bio gracza z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbGetBalance(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\"");
                    return;
                }

                double amount = playerDataManager.GetPlayerBalance(playerData);
                UnturnedChat.Say(caller, $"Gracz \"{command[0]}\" ma ${amount} w portfelu");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się pobrać ilości środków gracza z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbSetBalance(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\"");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    UnturnedChat.Say(caller, "Musisz podać ilość środków do ustawienia");
                    return;
                }

                playerDataManager.SetPlayerBalance(playerData, amount);
                UnturnedChat.Say(caller, "Ustawiono ilość środków gracza");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić ilości środków gracza z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbDeposit(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\"");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    UnturnedChat.Say(caller, "Musisz podać ilość środków do zdeponowania");
                    return;
                }

                playerDataManager.DepositIntoWallet(playerData, amount);
                UnturnedChat.Say(caller, "Zdeponowano środki do portfela gracza");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zdeponować środków do portfela gracza z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbWithdraw(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Musisz podać nazwę lub ID gracza");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData = playerDataManager.ResolvePlayer(command[0], false);

                if (playerData == null)
                {
                    UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[0]}\"");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    UnturnedChat.Say(caller, "Musisz podać ilość środków do wypłacenia");
                    return;
                }

                playerDataManager.WithdrawFromWallet(playerData, amount);
                UnturnedChat.Say(caller, "Wypłacono środki z portfela gracza");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się wypłacić środków z portfela gracza z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
