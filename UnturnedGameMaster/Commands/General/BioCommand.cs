using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.General
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

            if (playerData == null)
            {
                ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                return;
            }

            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Twoje publiczne bio: \"{playerData.Bio}\"");
                return;
            }
            else
            {
                playerData.SetBio(string.Join(" ", command));
                ChatHelper.Say(caller, $"Ustawiono twoje bio na: \"{playerData.Bio}\"");

                return;
            }
        }
    }
}
