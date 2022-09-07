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

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManageWorldSpawnCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "worldspawn";

        public string Help => "";

        public string Syntax => "<set/reset>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, $"Musisz podać argument.");
                ShowSyntax(caller);
                return;
            }

            switch (command[0].ToLowerInvariant())
            {
                case "set":
                    VerbSet(caller);
                    break;
                case "reset":
                    VerbReset(caller);
                    break;
                default:
                    UnturnedChat.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            UnturnedChat.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbSet(IRocketPlayer caller)
        {
            try
            {
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);
                RespawnManager respawnManager = ServiceLocator.Instance.LocateService<RespawnManager>();

                VectorPAR? respawnPoint = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);
                respawnManager.SetWorldRespawnPoint(respawnPoint);

                UnturnedChat.Say(caller, "Ustawiono spawn świata");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się ustawić spawn świata z powodu błedu serwera: {ex.Message}");
                return;
            }
        }

        private void VerbReset(IRocketPlayer caller)
        {
            try
            {
                RespawnManager respawnManager = ServiceLocator.Instance.LocateService<RespawnManager>();
                respawnManager.SetWorldRespawnPoint(null);

                UnturnedChat.Say(caller, "Zresetowano spawn świata");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się zresetować spawn świata z powodu błedu serwera: {ex.Message}");
                return;
            }
        }
    }
}
