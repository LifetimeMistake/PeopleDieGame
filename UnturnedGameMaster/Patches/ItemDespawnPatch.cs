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
using SDG.NetTransport;
using UnturnedGameMaster.Helpers;

namespace UnturnedGameMaster.Patches
{
    [HarmonyPatch(typeof(ItemManager), "despawnItems")]
    public static class ItemDespawnPatch
    {
        public static bool Prefix(ItemManager __instance, ref bool __result)
        {
			ObjectiveManager objectiveManager = ServiceLocator.Instance.LocateService<ObjectiveManager>();

			byte despawnItems_X = (byte)AccessTools.Field(__instance.GetType(), "despawnItems_X").GetValue(__instance);
			byte despawnItems_Y = (byte)AccessTools.Field(__instance.GetType(), "despawnItems_Y").GetValue(__instance);
			MethodInfo SendDestroyItemMethod = AccessTools.Method(__instance.GetType(), "SendDestroyItem");

			CachedItem[] cachedItems = objectiveManager.GetCachedItems();

			List<ItemData> itemList;

			if (cachedItems.Length > 0)
				itemList = (List<ItemData>)ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items
					.Where(x => !cachedItems.Any(y => y.RegionItem.ItemData.instanceID == x.instanceID));
			else
				itemList = ItemManager.regions[(int)despawnItems_X, (int)despawnItems_Y].items;

			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				__result = false;
			}
			if (itemList.Count > 0)
			{
				for (int i = 0; i < itemList.Count; i++)
				{
					if (Time.realtimeSinceStartup - itemList[i].lastDropped > (itemList[i].isDropped ? Provider.modeConfigData.Items.Despawn_Dropped_Time : Provider.modeConfigData.Items.Despawn_Natural_Time))
					{
						uint instanceID = itemList[i].instanceID;
						itemList.RemoveAt(i);
						SendDestroyItemMethod.Invoke(__instance, new object[] { ENetReliability.Reliable, Regions.EnumerateClients(despawnItems_X, despawnItems_Y, ItemManager.ITEM_REGIONS), despawnItems_X, despawnItems_Y, instanceID, false });
						break;
					}
				}
				__result = true;
			}
			__result = false;

			return false;
		}
    }
}
