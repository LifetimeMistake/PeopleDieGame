using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Patches
{
    [HarmonyPatch(typeof(NetReflection), "GetServerMethodInfo", new Type[] { typeof(Type), typeof(string) })]
    public class NetReflection_GetServerMethodInfo_Patch
    {
        public static bool Prefix(Type declaringType, string methodName, ref ServerMethodInfo __result)
        {
            __result = CustomNetReflection.GetServerMethodInfo(declaringType, methodName);
            return false;
        }
    }
}
