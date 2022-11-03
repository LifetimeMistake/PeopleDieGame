using HarmonyLib;
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
    [HarmonyPatch(typeof(Items), "addItem")]
    public static class AltarStoringPatch
    {
        public static bool Prefix(Items __instance, ref Item item)
        {
            AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();
            if (!altarManager.Receptacles.Any(x => x.items == __instance))
                return true;

            ushort itemId = item.id;
            ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();
            if (!objectiveManager.GetObjectiveItems().Any(x => x.ItemId == itemId))
            {
                return false;
            }
                
            return true;
        }
    }
}
