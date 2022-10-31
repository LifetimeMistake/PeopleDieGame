using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods
{
    public static class NetMethodsLoader
    {
        private static Harmony harmony;
        public static void Load()
        {
            harmony = new Harmony("PeopleDieGame.NetMethods");
            harmony.PatchAll();
            Debug.Log($"NetMethods: Patched {harmony.GetPatchedMethods().Count()} game methods");

            Debug.Log("Registering custom RPC calls...");
            NetReflection.SetLogCallback(Log);
            int customCallCount = CustomNetReflection.RegisterCustomRPCs();
            Debug.Log($"Registered {customCallCount} calls!");
        }

        public static void Unload()
        {
            harmony.UnpatchAll();
        }

        private static void Log(string text)
        {
            Debug.Log(text);
        }
    }
}
