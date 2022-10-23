using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Commands.Shop
{
    public class BalanceCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "balance";

        public string Help => "Pokazuje wartość portfela gracza";

        public string Syntax => "";

        public List<string> Aliases => new List<string>() { "bal"};

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            try
            {
                UnturnedPlayer player = (UnturnedPlayer)caller;
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
                if (playerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Balans twojego portfela wynosi: ${playerData.WalletBalance}");
                if (playerData.TeamId.HasValue)
                {
                    TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                    Team team = teamManager.GetTeam(playerData.TeamId.Value);
                    if (team != null)
                    {
                        stringBuilder.AppendLine($"Balans portfela twojej drużyny wynosi: ${team.BankBalance}");
                    }
                }
                ChatHelper.Say(caller, stringBuilder);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się odzczytać portfelu gracza z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
