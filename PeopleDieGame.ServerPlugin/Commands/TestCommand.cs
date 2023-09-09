using PeopleDieGame.NetMethods.RPCs;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Commands
{
    public class TestCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "augh";

        public string Help => "";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            SteamPlayer player = ((UnturnedPlayer)caller).SteamPlayer();
            if (command[0] == "no")
            {
                BossBarManager.RemoveBossBar(player);
                return;
            }

            BossBarManager.UpdateBossBar(command[0], float.Parse(command[1]), player);
        }
    }
}
