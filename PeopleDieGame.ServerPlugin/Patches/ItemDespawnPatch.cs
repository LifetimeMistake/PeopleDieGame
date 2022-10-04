using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using UnityEngine;
using SDG.NetTransport;
using PeopleDieGame.ServerPlugin.Helpers;
using System.Diagnostics;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(ItemManager), "despawnItems")]
    public static class ItemDespawnPatch
    {
        public static bool Prefix(ItemManager __instance, ref bool __result)
        {
            ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();

            byte despawnItems_X = (byte)AccessTools.Field(__instance.GetType(), "despawnItems_X").GetValue(__instance);
            byte despawnItems_Y = (byte)AccessTools.Field(__instance.GetType(), "despawnItems_Y").GetValue(__instance);
            FieldInfo SendDestroyItemFieldInfo = __instance.GetType().GetField("SendDestroyItem", BindingFlags.Static | BindingFlags.NonPublic);

            ClientStaticMethod<byte, byte, uint, bool> SendDestroyItem = SendDestroyItemFieldInfo.GetValue(null) as ClientStaticMethod<byte, byte, uint, bool>;

            CachedItem[] cachedItems = objectiveManager.GetCachedItems();

            List<ItemData> itemList = ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items.Where(x =>
            !cachedItems.Any(y => y.GetLocation(false) == Enums.CachedItemState.Ground && y.RegionItem.ItemData.instanceID == x.instanceID)).ToList();

            if (Level.info == null || Level.info.type == ELevelType.ARENA)
            {
                __result = false;
                return false;
            }

            if (itemList.Count > 0)
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (Time.realtimeSinceStartup - itemList[i].lastDropped > (itemList[i].isDropped ? Provider.modeConfigData.Items.Despawn_Dropped_Time : Provider.modeConfigData.Items.Despawn_Natural_Time))
                    {
                        uint instanceID = itemList[i].instanceID;
                        itemList.RemoveAt(i);
                        SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(despawnItems_X, despawnItems_Y, ItemManager.ITEM_REGIONS), despawnItems_X, despawnItems_Y, instanceID, false);
                        break;
                    }
                }

                __result = true;
                return false;
            }

            __result = false;
            return false;
        }
    }
}
