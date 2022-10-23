using HarmonyLib;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(StructureManager), "dropReplicatedStructure")]
    public static class ArenaStructurePatch
    {
        public static bool Prefix(ref Vector3 point, ref bool __result)
        {
            ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
            foreach (BossArena arena in arenaManager.GetArenas())
            {
                if (Vector3.Distance(arena.ActivationPoint, point) <= arena.DeactivationDistance)
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}
