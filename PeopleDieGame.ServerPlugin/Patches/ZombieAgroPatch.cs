using HarmonyLib;
using SDG.Unturned;
using System;
using System.Linq;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(ZombieRegion), "HasInfiniteAgroRange")]
    [HarmonyPatch(MethodType.Getter)]
    public static class ZombieAgroPatch
    {
        public static bool Prefix(ZombieRegion __instance, out bool __result)
        {
            if (ServiceLocator.Instance != null)
            {
                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
                if (arenaManager != null)
                {
                    int boundId = Array.FindIndex(ZombieManager.regions, x => x == __instance);
                    // Make it so zombies always get infinite aggro during boss fights
                    if (arenaManager.GetBossFights().Any(x => x.Arena.BoundId == boundId))
                    {
                        __result = true;
                        return false;
                    }
                }
            }

            __result = false;
            return true;
        }
    }
}