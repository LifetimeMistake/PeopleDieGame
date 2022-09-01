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
            if (!Physics.Raycast(ray, out hit, 100, RayMasks.LARGE))
            {
                UnturnedChat.Say("no luck");
                return;
            }

            UnturnedChat.Say($"transform at {hit.transform.position}");
        }
    }
}
