using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.General
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
            PlayerData playerData = playerDataManager.GetData((ulong)((UnturnedPlayer)caller).CSteamID);
            if (playerData == null)
            {
                ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                return;
            }

            playerDataManager.UpdateBio(playerData, "");
            ChatHelper.Say(caller, "Zresetowano twoje bio!");
        }
    }
}
