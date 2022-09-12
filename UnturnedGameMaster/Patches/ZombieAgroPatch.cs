using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;

namespace UnturnedGameMaster.Patches
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