using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
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
                ChatHelper.Say(caller, $"Musisz podać argument.");
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
                    ChatHelper.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbSet(IRocketPlayer caller)
        {
            try
            {
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);
                RespawnManager respawnManager = ServiceLocator.Instance.LocateService<RespawnManager>();

                VectorPAR? respawnPoint = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);
                respawnManager.SetWorldRespawnPoint(respawnPoint);

                ChatHelper.Say(caller, "Ustawiono spawn świata");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić spawn świata z powodu błedu serwera: {ex.Message}");
            }
        }

        private void VerbReset(IRocketPlayer caller)
        {
            try
            {
                RespawnManager respawnManager = ServiceLocator.Instance.LocateService<RespawnManager>();
                respawnManager.SetWorldRespawnPoint(null);

                ChatHelper.Say(caller, "Zresetowano spawn świata");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zresetować spawn świata z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}
