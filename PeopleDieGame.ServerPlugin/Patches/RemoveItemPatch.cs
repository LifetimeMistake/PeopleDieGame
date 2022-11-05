using HarmonyLib;
using PeopleDieGame.Reflection;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
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
    [HarmonyPatch(typeof(Items), "removeItem")]
    public static class RemoveItemPatch
    {
        public static bool Prefix(Items __instance)
		{
            AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();
            if (altarManager.GetReceptacles().Any(x => x.items == __instance))
                return false;
            return true;
		}
    }
}
