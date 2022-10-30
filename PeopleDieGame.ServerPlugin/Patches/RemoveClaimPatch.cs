using HarmonyLib;
using PeopleDieGame.Reflection;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
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
    [HarmonyPatch(typeof(InteractableClaim), "OnDisable")]
    public static class RemoveClaimPatch
    {
        public static void Postfix(ref InteractableClaim __instance)
        {
            TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
            Team team = teamManager.GetTeamByGroup((CSteamID)__instance.group);

            if (team == null)
                throw new Exception($"Player {__instance.owner} attempted to place a claimflag (does not belong to a team)");

            teamManager.RemoveBaseClaim(team);
        }
    }
}
