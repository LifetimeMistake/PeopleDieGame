using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.General
{
    public class ResetBioCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "resetbio";

        public string Help => "Resetuje bio gracza na domyślne.";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
            PlayerData playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
            if (playerData == null)
            {
                UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                return;
            }

            playerData.SetBio("");
            UnturnedChat.Say(caller, "Zresetowano twoje bio!");
        }
    }
}
