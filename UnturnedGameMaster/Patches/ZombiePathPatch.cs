﻿using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Patches
{
    [HarmonyPatch(typeof(Zombie), "alert", new[] { typeof(Player) })]
    public static class ZombiePathPatch
    {
        public static void Postfix(Zombie __instance, ref EZombiePath ___path)
        {
            ManagedZombie zombie = __instance as ManagedZombie;
            if (zombie == null)
                return;

            if (zombie.PathOverride.HasValue)
                ___path = zombie.PathOverride.Value;
        }
    }
}