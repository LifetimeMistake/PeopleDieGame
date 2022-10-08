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
    [HarmonyPatch(typeof(BarricadeManager), "damage")]
    public static class ReceptacleDamagePatch
    {
        public static bool Prefix(BarricadeManager __instance, Transform transform)
        {
			BarricadeRegion barricadeRegion;
			if (!BarricadeManager.tryGetRegion(transform, out byte x, out byte y, out ushort num, out barricadeRegion))
			{
				return false;
			}
			BarricadeDrop barricadeDrop = barricadeRegion.FindBarricadeByRootTransform(transform);
			if (barricadeDrop == null)
			{
				return false;
			}

			AltarManager altarManager = ServiceLocator.Instance.LocateService<AltarManager>();
			InteractableStorage storage = barricadeDrop.interactable as InteractableStorage;

			if (storage == null)
				return true;

			if (altarManager.GetAltar().Receptacles.Contains(storage))
				return false;

			return true;
		}
    }
}
