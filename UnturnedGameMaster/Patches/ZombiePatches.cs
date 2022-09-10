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

            // to be continued
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
