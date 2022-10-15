using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using Rocket.API;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Commands
{
    public class TestSubmitCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "submit";

        public string Help => "";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();
                PlayerData callerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);

                if (!altarManager.IsPlayerInAltar(callerData))
                {
                    ChatHelper.Say("Nie znajdujesz się w obszarze altar'u");
                    return;
                }

                if (!altarManager.SubmitItems(callerData))
                {
                    ChatHelper.Say("you failed, booo");
                    return;
                }
                ChatHelper.Say("waow");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"augh: {ex.Message}");
            }
        }
    }
}
