using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.Exception;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.Admin
{
    public class GiveLoadoutCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "giveloadout";

        public string Help => "";

        public string Syntax => "<loadoutname/loadoutid> [<playername/playerid>";

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

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                PlayerData playerData;

                Loadout loadout = loadoutManager.ResolveLoadout(command[0], false);
                if (loadout == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono zestawu");
                    return;
                }

                if (command.Length == 1)
                {
                    playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                    if (playerData == null)
                    {
                        ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                        return;
                    }
                }
                else
                {
                    playerData = playerDataManager.ResolvePlayer(command[1], false);
                    if (playerData == null)
                    {
                        ChatHelper.Say(caller, $"Nie znaleziono gracza \"{command[1]}\"");
                        return;
                    }
                }


                loadoutManager.GiveLoadout(playerData, loadout);
                ChatHelper.Say(caller, "Nadano zestaw wyposażenia");
            }
            catch (PlayerOfflineException)
            {
                ChatHelper.Say(caller, "Gracz docelowy nie znajduje się obecnie na serwerze.");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się nadać zestawu z powodu błedu serwera: {ex.Message}");
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }
    }
}
