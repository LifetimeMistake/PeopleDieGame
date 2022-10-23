using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(BarricadeManager), "dropBarricade")]
    public static class ArenaBarricadePatch
    {
        public static bool Prefix(ref Vector3 point, ref Transform __result)
        {
            ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();
            foreach (BossArena arena in arenaManager.GetArenas())
            {
                if (Vector3.Distance(arena.ActivationPoint, point) <= arena.DeactivationDistance)
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }
}