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
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.ServerPlugin.Reflection;

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
            List<ItemData> itemList = ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items;

            if (Level.info == null || Level.info.type == ELevelType.ARENA)
            {
                __result = false;
                return false;
            }

            if (itemList.Count > 0)
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    ItemData item = itemList[i];
                    uint instanceID = item.instanceID;
                    if (cachedItems.Any(x => x.GetLocation() == CachedItemState.Ground && x.RegionItem.ItemData.instanceID == instanceID))
                    {
                        FieldRef<float> lastDropped = FieldRef.GetFieldRef<ItemData, float>(item, "_lastDropped");
                        lastDropped.Value = Time.realtimeSinceStartup;
                        continue;
                    }

                    if (Time.realtimeSinceStartup - item.lastDropped > (itemList[i].isDropped ? Provider.modeConfigData.Items.Despawn_Dropped_Time : Provider.modeConfigData.Items.Despawn_Natural_Time))
                    {
                        itemList.RemoveAt(i);
                        SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(despawnItems_X, despawnItems_Y, ItemManager.ITEM_REGIONS), despawnItems_X, despawnItems_Y, instanceID, false);
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
