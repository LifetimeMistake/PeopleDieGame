using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.Admin
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
