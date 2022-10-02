using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ZombieCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "zombie";

        public string Help => "Spawnuje zombie";

        public string Syntax => "<type> <hat id> <shirt id> <pants id> <gear id>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 5)
            {
                ChatHelper.Say(caller, "Nie podano wystarczającej ilości argumentów");
                ShowSyntax(caller);
                return;
            }

            if (!Enum.TryParse(command[0], true, out EZombieSpeciality speciality))
            {
                ChatHelper.Say(caller, $"Nieprawidłowy typ zombie, dozwolone wartości: {string.Join(", ", Enum.GetNames(typeof(EZombieSpeciality)))}");
                return;
            }

            if (!byte.TryParse(command[1], out byte hatId))
            {
                ChatHelper.Say(caller, "Niepoprawne ID czapki");
                return;
            }

            if (!byte.TryParse(command[1], out byte shirtId))
            {
                ChatHelper.Say(caller, "Niepoprawne ID koszuli");
                return;
            }

            if (!byte.TryParse(command[1], out byte pantsId))
            {
                ChatHelper.Say(caller, "Niepoprawne ID spodni");
                return;
            }

            if (!byte.TryParse(command[1], out byte gearId))
            {
                ChatHelper.Say(caller, "Niepoprawne ID akcesorium");
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (!LevelNavigation.tryGetBounds(player.Position, out byte boundId))
            {
                ChatHelper.Say(caller, "W pobliżu nie ma dostępnej siatki nawigacji zombie.");
                return;
            }

            ZombiePoolManager zombiePoolManager = ServiceLocator.Instance.LocateService<ZombiePoolManager>();
            if (!zombiePoolManager.ZombiePoolExists(boundId))
            {
                ChatHelper.Say(caller, "Nie znajdujesz się w zarządzalnej siatce nawigacji zombie.");
                return;
            }

            RaycastHit hit;
            Ray ray = new Ray(player.Player.look.aim.position, player.Player.look.aim.forward);
            if (!Physics.Raycast(ray, out hit, 100, RayMasks.GROUND | RayMasks.LARGE))
            {
                ChatHelper.Say(caller, "Wskazano nieprawidłową pozycję spawnu");
                return;
            }

            ManagedZombie zombie = zombiePoolManager.SpawnZombie(boundId, 0, (byte)speciality, shirtId, pantsId, hatId, gearId, hit.point, 0, true);
            if (zombie == null)
            {
                ChatHelper.Say(caller, "Nie udało się zespawnować zombie");
                return;
            }

            zombie.AIEnabled = false;
            ChatHelper.Say(caller, "ok");
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }
    }
}
