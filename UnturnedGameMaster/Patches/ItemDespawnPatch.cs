using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;
using UnityEngine;

namespace UnturnedGameMaster.Patches
{
    [HarmonyPatch(typeof(ItemManager), "despawnItem")]
    public static class ItemDespawnPatch
    {
        public static bool Prefix(ItemManager __instance, ref bool __result)
        {
			byte despawnItems_X = (byte)__instance.GetType().GetField("despawnItems_X").GetValue(__instance);
			byte despawnItems_Y = (byte)__instance.GetType().GetField("despawnItems_Y").GetValue(__instance);
			MethodInfo SendDestroyItemMethod = __instance.GetType().GetMethod("SendDestroyItem");

			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				return false;
			}
			if (ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items.Count > 0)
			{
				for (int i = 0; i < ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items.Count; i++)
				{
					if (Time.realtimeSinceStartup - ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items[i].lastDropped > (ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items[i].isDropped ? Provider.modeConfigData.Items.Despawn_Dropped_Time : Provider.modeConfigData.Items.Despawn_Natural_Time))
					{
						uint instanceID = ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items[i].instanceID;
						ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items.RemoveAt(i);
						//SendDestroyItemMethod.Invoke(SDG.NetTransporENetReliability.Reliable, Regions.EnumerateClients(despawnItems_X, despawnItems_Y, ItemManager.ITEM_REGIONS), despawnItems_X, despawnItems_Y, instanceID, false);
						break;
					}
				}
				return true;
			}
			return false;
		}
    }
}
