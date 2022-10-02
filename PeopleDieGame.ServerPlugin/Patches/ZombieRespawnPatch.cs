using HarmonyLib;
using SDG.Unturned;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Models;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(Zombie), "sendRevive", new[] { typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(Vector3), typeof(float) })]
    public static class ZombieRespawnPatch
    {
        public static bool Prefix(Zombie __instance)
        {
            // Make it so it's impossible for the internal ZombieManager to respawn managed zombies,
            // which should be respawned using the ZombiePoolManager instead
            return !(__instance is ManagedZombie);
        }
    }
}
