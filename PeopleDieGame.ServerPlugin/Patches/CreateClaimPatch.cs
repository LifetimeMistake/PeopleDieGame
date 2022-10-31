using HarmonyLib;
using PeopleDieGame.Reflection;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using PeopleDieGame.ServerPlugin.Services.Providers;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(BarricadeManager), "dropBarricade")]
    public static class CreateClaimPatch
    { 
        public static bool Prefix(ref Barricade barricade, ref ulong owner, ref ulong group)
        {
			if (barricade.asset.build != EBuild.CLAIM)
				return true;

			TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
			Team team = teamManager.GetTeamByGroup((CSteamID)group);

			PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
			PlayerData playerData = playerDataManager.GetPlayer(owner);

			if (team == null || team.LeaderId.Value != playerData.Id)
            {
				ChatHelper.Say(playerData, "Nie możesz postawić flagi zbazowania nie będąc liderem drużyny (nie jesteś zbazowany lmao)");
				return false;
            }
				
			if (teamManager.HasClaim(team))
            {
				ChatHelper.Say(playerData, "Twoja drużyna posiada już bazę!");
				return false;
			}

			return true;
		}
    }
}
