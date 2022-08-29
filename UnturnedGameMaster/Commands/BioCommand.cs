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

namespace UnturnedGameMaster.Commands
{
    public class BioCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "bio";

        public string Help => "Pokazuje obecne bio lub ustawia nowe.";

        public string Syntax => "[<bio>]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
            PlayerData playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
            if(playerData == null)
            {
                UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć akt gracza??)");
                return;
            }

            if(command.Length == 0)
            {
                UnturnedChat.Say(caller, $"Twoje publiczne bio: {playerData.Bio}");
                return;
            }
            else
            {
                playerData.SetBio(string.Join(" ", command));
                UnturnedChat.Say(caller, $"Ustawiono twoje bio na: {playerData.Bio}");
                return;
            }
        }
    }
}
