using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Framework.Utilities;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class ManageAltarCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "altar";

        public string Help => "";

        public string Syntax => "<inspect/setpos/setradius/addreceptacle/resetreceptacles> <radius>";

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

            string[] verbArgs = command.Skip(1).ToArray();
            switch (command[0].ToLowerInvariant())
            {
                case "inspect":
                    VerbInspect(caller);
                    break;
                case "setpos":
                    VerbSetPos(caller);
                    break;
                case "setradius":
                    VerbSetRadius(caller, verbArgs);
                    break;
                case "addreceptacle":
                    VerbAddReceptacle(caller);
                    break;
                case "resetreceptacles":
                    VerbResetReceptacles(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbInspect(IRocketPlayer caller)
        {
            try
            {
                AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();
                Altar altar = altarManager.GetAltar();

                StringBuilder sb = new StringBuilder();

                if (!altar.Position.HasValue)
                    sb.AppendLine("Uwaga: Altar nie posiada pozycji");
                else
                    sb.AppendLine($"Pozycja: {altar.Position}");

                sb.AppendLine($"Promień obszaru: {altar.Radius}");

                if (altar.Receptacles.Count == 0)
                {
                    sb.AppendLine("Altar nie posiada pojemników");
                } 
                else
                {
                    sb.AppendLine("Pojemniki:");
                    foreach (InteractableStorage storage in altar.Receptacles)
                    {
                        sb.AppendLine($"\tID: {storage.name} | {storage.transform.position}");
                    }
                }

                ChatHelper.Say(sb);
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się pobrać informacji o altarze z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetPos(IRocketPlayer caller)
        {
            try
            {
                AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);

                altarManager.SetAltarPosition(callerPlayer.Position);

                ChatHelper.Say(caller, "Ustawiono pozycję altar'u");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić pozycji altar'u z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetRadius(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać promień obszaru altar'u");
                return;
            }

            if (!double.TryParse(command[0], out double radius))
            {
                ChatHelper.Say(caller, "Musisz podać odpowiedni promień obszaru altar'u");
                return;
            }

            try
            {
                AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();

                altarManager.SetAltarRadius(radius);

                ChatHelper.Say(caller, "Ustawiono promień obszaru altar'u");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się ustawić promienia obszaru altar'u z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbAddReceptacle(IRocketPlayer caller)
        {
            try
            {
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);

                PlayerLook look = callerPlayer.Player.look;
                if (!Physics.Raycast(new Ray(look.aim.position, look.aim.forward), out RaycastHit hit, 10, RayMasks.BARRICADE_INTERACT))
                {
                    ChatHelper.Say(caller, "Musisz patrzyć się na pojemnik by dodać go do altar'u");
                    return;
                }

                InteractableStorage storage = hit.transform.GetComponent<InteractableStorage>();
                if (storage == null)
                {
                    ChatHelper.Say(caller, "Obiekt nie jest pojemnikiem");
                    return;
                }

                AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();

                altarManager.AddReceptacle(storage);

                ChatHelper.Say(caller, "Dodano pojemnik");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się dodać pojemnika do altar'u z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbResetReceptacles(IRocketPlayer caller)
        {
            try
            {
                AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();
                
                if (!altarManager.ResetReceptacles())
                {
                    ChatHelper.Say(caller, "Altar nie posiada pojemników");
                    return;
                }

                ChatHelper.Say(caller, "Zresetowano pojemniki altar'u");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zresetować pojemników altar'u z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
