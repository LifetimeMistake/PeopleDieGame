using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Framework.Translations;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Helpers;

namespace UnturnedGameMaster.Commands
{
    public class TestCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "test";

        public string Help => "";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Player player = ((UnturnedPlayer)caller).Player;
            RaycastHit hit;
            Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
            if (!Physics.Raycast(ray, out hit, 100, RayMasks.GROUND))
            {
                ChatHelper.Say("no luck");
                return;
            }

            byte boundId;

            if (!LevelNavigation.tryGetBounds(hit.point, out boundId))
            {
                UnturnedChat.Say("hwhat");
                return;
            }
        }
    }
}
