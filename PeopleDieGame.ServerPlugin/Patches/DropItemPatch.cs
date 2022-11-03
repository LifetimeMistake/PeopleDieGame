using HarmonyLib;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Services.Managers;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(ItemManager), "dropItem")]
    public static class DropItemPatch
    {
        public static bool Prefix(ref Item item)
        {
            ushort itemId = item.id;
            ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();
            if (objectiveManager.GetObjectiveItems().Any(x => x.ItemId == itemId && x.State == Enums.ObjectiveState.Secured))
                return false;
            return true;
        }
    }
}
