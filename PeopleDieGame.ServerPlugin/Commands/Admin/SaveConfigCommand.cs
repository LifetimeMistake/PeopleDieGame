using Rocket.API;
using System.Collections.Generic;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class SaveConfigCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "saveconfig";

        public string Help => "";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            DataManager dataManager = ServiceLocator.Instance.LocateService<DataManager>();
            ChatHelper.Say(caller, "Success: " + dataManager.CommitConfig());
        }
    }
}
