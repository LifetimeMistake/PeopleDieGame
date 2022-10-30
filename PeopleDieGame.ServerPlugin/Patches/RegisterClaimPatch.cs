using HarmonyLib;
using PeopleDieGame.Reflection;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Services.Managers;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ServerPlugin.Patches
{
	[HarmonyPatch(typeof(InteractableClaim), "registerClaim")]
	public static class RegisterClaimPatch
    {
		public static bool Prefix(ref InteractableClaim __instance)
		{
			FieldRef<ClaimPlant> _plant = FieldRef.GetFieldRef<InteractableClaim, ClaimPlant>(__instance, "plant");
			FieldRef<ClaimBubble> _bubble = FieldRef.GetFieldRef<InteractableClaim, ClaimBubble>(__instance, "bubble");

			ClaimPlant plant = _plant.Value;
			ClaimBubble bubble = _bubble.Value;

			TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
			Team team = teamManager.GetTeamByGroup((CSteamID)__instance.group);

			if (__instance.isPlant)
			{
				if (plant == null)
				{
					plant = ClaimManager.registerPlant(__instance.transform.parent, __instance.owner, __instance.group);
					return false;
				}
			}
			else if (bubble == null)
			{
				bubble = ClaimManager.registerBubble(__instance.transform.position, 32f, __instance.owner, __instance.group);
				teamManager.SetBaseClaim(team, bubble);
			}

			return false;
		}
    }
}
