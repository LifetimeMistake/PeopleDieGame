using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Patches
{
    [HarmonyPatch(typeof(Zombie), "tick")]
    public static class ZombieAbilityPatch
    {
        private readonly static List<OpCode> startOpcodes = new List<OpCode>
        {
            // player != null
            OpCodes.Ldarg_0,
            OpCodes.Ldfld,
            OpCodes.Ldnull,
            OpCodes.Call,
            OpCodes.Brtrue_S,
            // barricade != null
            OpCodes.Ldarg_0,
            OpCodes.Ldfld,
            OpCodes.Ldnull,
            OpCodes.Call,
            OpCodes.Brtrue_S,
            // structure != null
            OpCodes.Ldarg_0,
            OpCodes.Ldfld,
            OpCodes.Ldnull,
            OpCodes.Call,
            OpCodes.Brtrue_S,
            // targetObstructionVehicle != null
            OpCodes.Ldarg_0,
            OpCodes.Ldfld,
            OpCodes.Ldnull,
            OpCodes.Call,
            OpCodes.Brtrue_S,
            // targetPassengerVehicle != null
            OpCodes.Ldarg_0,
            OpCodes.Ldfld,
            OpCodes.Ldnull,
            OpCodes.Call,
            OpCodes.Brfalse
        };

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> opCodes = new List<CodeInstruction>(codeInstructions);
            int startIndex = opCodes.Select(x => x.opcode).GetSubsequenceIndex(startOpcodes) + startOpcodes.Count;
            if (startIndex == -1)
                throw new Exception("Failed to find patch start");

            MethodInfo sendZombieChargeInfo = AccessTools.Method(typeof(ZombieManager), "sendZombieCharge");
            if (sendZombieChargeInfo == null)
                throw new Exception("Failed to find ZombieManager.sendZombieCharge method");

            int endIndex = opCodes.GetRange(startIndex, opCodes.Count - startIndex).FindIndex(x => x.opcode == OpCodes.Call && (MethodInfo)x.operand == sendZombieChargeInfo) + startIndex;
            if (endIndex == -1)
                throw new Exception("Failed to find patch end");

            for (int i = startIndex; i <= endIndex; i++)
            {
                opCodes[i].opcode = OpCodes.Nop;
                opCodes[i].operand = null;
            }

            List<CodeInstruction> newOpCodes = new List<CodeInstruction>
            {
                // Zombie zombie
                new CodeInstruction(OpCodes.Ldarg_0), 
                // targetDistance
                new CodeInstruction(OpCodes.Ldloc_1),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "boulderThrowDelay")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "isThrowRelocating")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "lastAttack")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "lastRelocate")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "lastStartle")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "lastSpecial")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "player")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "seeker")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "specialStartleDelay")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "specialUseDelay")),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Zombie), "specialAttackDelay")),

                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ManagedZombie), "UpdateAttacks"))
            };

            for (int i = 0; i < newOpCodes.Count; i++)
            {
                CodeInstruction codeInstruction = newOpCodes[i];
                opCodes[startIndex + i].Replace(codeInstruction.opcode, codeInstruction.operand);
            }

            return opCodes;
        }
    }

    public static class CodeInstructionExtensions
    { 
        public static void Replace(this CodeInstruction codeInstruction, OpCode opCode, object operand = null)
        {
            codeInstruction.opcode = opCode;
            codeInstruction.operand = operand;
        }
    }
}
