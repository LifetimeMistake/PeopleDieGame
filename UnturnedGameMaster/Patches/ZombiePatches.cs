using HarmonyLib;
using Mono.Cecil.Cil;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Patches
{
    [HarmonyPatch(typeof(Zombie), "tick")]
    public static class ZombieAbilityPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator iLGenerator)
        {
            int startIndex = 807;
            int endIndex = 1161;

            List<CodeInstruction> opCodes = new List<CodeInstruction>(codeInstructions);
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
        public static void Replace(this CodeInstruction codeInstruction, System.Reflection.Emit.OpCode opCode, object operand = null)
        {
            codeInstruction.opcode = opCode;
            codeInstruction.operand = operand;
        }
    }
}
