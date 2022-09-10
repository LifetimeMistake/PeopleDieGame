using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

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
            Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100, RayMasks.GROUND))
            {
                ChatHelper.Say("no luck");
                return;
            }

            UnturnedChat.Say($"{hit.point}");
            byte boundId;

            if (!LevelNavigation.tryGetBounds(hit.point, out boundId))
            {
                UnturnedChat.Say("hwhat");
                return;
            }

            ZombiePoolManager zombiePoolManager = ServiceLocator.Instance.LocateService<ZombiePoolManager>();
            if (!zombiePoolManager.ZombiePoolExists(boundId))
            {
                UnturnedChat.Say($"creating pool: {zombiePoolManager.CreateZombiePool(boundId, 10)}");
                return;
            }

            byte shirt = (byte)Random.Range(0, 3);
            byte pants = (byte)Random.Range(0, 3);
            byte hat = (byte)Random.Range(0, 3);
            byte gear = (byte)Random.Range(0, 3);
            bool force = command.Length == 1 && command[0] == "yes";

            ManagedZombie result = zombiePoolManager.SpawnZombie(boundId, 0, (byte)EZombieSpeciality.BOSS_MAGMA, shirt, pants, hat, gear, hit.point + new Vector3(0, 1, 0), 0, force);
            if (result)
            {
                result.SetHealth(1);
                result.SetMaxHealth(1);
            }
        }
    }
}
