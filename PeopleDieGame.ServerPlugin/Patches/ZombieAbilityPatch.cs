using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Enums;

namespace PeopleDieGame.ServerPlugin.Patches
{
    [HarmonyPatch(typeof(Zombie), "tick")]
    public static class ZombieAbilityPatch
    {
        private static Type abilityEnumType;
        private static EnumInfo<int> abilityEnum;
        private static FieldRef<object> availableAbilityChoices;
        private static MethodRef availableAbilityChoices_Clear;
        private static MethodRef availableAbilityChoices_Add;

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            abilityEnumType = typeof(Zombie).GetNestedType("EAbilityChoice", BindingFlags.NonPublic | BindingFlags.Instance);
            if (abilityEnumType == null)
                throw new Exception("Could not find zombie choice enum type");
            abilityEnum = new EnumInfo<int>(abilityEnumType);

            Type abilityList = typeof(List<>).MakeGenericType(abilityEnumType);
            FieldInfo availableAbilityChoices_fieldInfo = typeof(Zombie).GetField("availableAbilityChoices", BindingFlags.NonPublic | BindingFlags.Static);
            if (availableAbilityChoices_fieldInfo == null)
                throw new Exception("Could not find zombie choice list field");
            availableAbilityChoices = FieldRef.GetFieldRef<object>(availableAbilityChoices_fieldInfo);

            MethodInfo abilityList_Clear_methodInfo = abilityList.GetMethod("Clear");
            if (abilityList_Clear_methodInfo == null)
                throw new Exception("Could not find List.Clear method");
            availableAbilityChoices_Clear = MethodRef.GetMethodRef(availableAbilityChoices.Value, abilityList_Clear_methodInfo);

            MethodInfo abilityList_Add_methodInfo = abilityList.GetMethod("Add");
            if (abilityList_Clear_methodInfo == null)
                throw new Exception("Could not find List.Add method");
            availableAbilityChoices_Add = MethodRef.GetMethodRef(availableAbilityChoices.Value, abilityList_Add_methodInfo);

            PropertyInfo abilityList_Count = abilityList.GetProperty("Count");
            if (abilityList_Count == null)
                throw new Exception("Could not find List.Count property");

            MethodInfo abilityList_get_Count = abilityList_Count.GetGetMethod(true);
            if (abilityList_get_Count == null)
                throw new Exception("Could not find List.Count getter");

            List<CodeInstruction> startOpcodes = new List<CodeInstruction> // patch includes this (Zombie.availableAbilityChoices.Clear())
            {
                new CodeInstruction(OpCodes.Ldsfld, availableAbilityChoices_fieldInfo),
                new CodeInstruction(OpCodes.Callvirt, abilityList_Clear_methodInfo)
            };
            List<CodeInstruction> endOpcodes = new List<CodeInstruction> // patch ends before this (if Zombie.availableAbilityChoices.Count > 0)
            {
                new CodeInstruction(OpCodes.Ldsfld, availableAbilityChoices_fieldInfo),
                new CodeInstruction(OpCodes.Callvirt, abilityList_get_Count),
                new CodeInstruction(OpCodes.Ldc_I4_0, null)
            };

            List<CodeInstruction> opCodes = new List<CodeInstruction>(codeInstructions);
            int startIndex = opCodes.GetSubsequenceIndex(startOpcodes, new Func<CodeInstruction, CodeInstruction, bool>((c1, c2) => c1.DeepEquals(c2)));
            if (startIndex == -1)
                throw new Exception("Failed to find patch start");

            int bodyLength = opCodes.GetRange(startIndex, opCodes.Count - startIndex).GetSubsequenceIndex(endOpcodes, new Func<CodeInstruction, CodeInstruction, bool>((c1, c2) => c1.DeepEquals(c2)));
            if (bodyLength == -1)
                throw new Exception("Failed to find patch end");
            bodyLength -= 1; // skip last opcode

            int endIndex = startIndex + bodyLength;

            List<CodeInstruction> newOpCodes = new List<CodeInstruction>
            {
                // Zombie zombie
                new CodeInstruction(OpCodes.Ldarg_0), 
                // float horizontalDistance
                new CodeInstruction(OpCodes.Ldloc_1),
                // pass fields directly to speed up execution
                // EZombieSpeciality this.speciality
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Zombie), "speciality")),
                // float this.lastFlashbang
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Zombie), "lastFlashbang")),
                // float this.flashbangDelay
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Zombie), "flashbangDelay")),
                // call override
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ZombieAbilityPatch), "UpdateAbilities"))
            };

            for (int i = startIndex; i <= endIndex; i++)
            {
                opCodes[i].Replace(OpCodes.Nop, null);
            }

            for (int i = 0; i < newOpCodes.Count; i++)
            {
                CodeInstruction codeInstruction = newOpCodes[i];
                opCodes[startIndex + i].Replace(codeInstruction.opcode, codeInstruction.operand);
            }

            return opCodes;
        }

        public static void UpdateAbilities(Zombie zombie, float horizontalDistance, EZombieSpeciality speciality, float lastFlashbang, float flashbangDelay)
        { 
            availableAbilityChoices_Clear.Invoke();
            ManagedZombie managedZombie = zombie as ManagedZombie;
            if (managedZombie != null && !managedZombie.AIEnabled)
                return;
            
            if ((managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Throw)) || (speciality == EZombieSpeciality.MEGA || speciality == EZombieSpeciality.BOSS_KUWAIT || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL) && horizontalDistance > 20f)
            {
                availableAbilityChoices_Add.Invoke(abilityEnum.MemberToValue("ThrowBoulder"));
            }
            if ((managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Spit)) || speciality == EZombieSpeciality.ACID || speciality == EZombieSpeciality.BOSS_NUCLEAR || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL)
            {
                availableAbilityChoices_Add.Invoke(abilityEnum.MemberToValue("SpitAcid"));
            }
            if ((managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Stomp)) || (speciality == EZombieSpeciality.BOSS_WIND || speciality == EZombieSpeciality.BOSS_BUAK_WIND || speciality == EZombieSpeciality.BOSS_ELVER_STOMPER || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL) && horizontalDistance < 144f)
            {
                availableAbilityChoices_Add.Invoke(abilityEnum.MemberToValue("Stomp"));
            }
            if ((managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Breath)) || (speciality == EZombieSpeciality.BOSS_FIRE || speciality == EZombieSpeciality.BOSS_MAGMA || speciality == EZombieSpeciality.BOSS_BUAK_FIRE || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL) && horizontalDistance < 529f)
            {
                availableAbilityChoices_Add.Invoke(abilityEnum.MemberToValue("BreatheFire"));
            }
            if ((managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Charge)) || (speciality == EZombieSpeciality.BOSS_ELECTRIC || speciality == EZombieSpeciality.BOSS_BUAK_ELECTRIC || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL) && horizontalDistance > 4f && horizontalDistance < 4096f)
            {
                availableAbilityChoices_Add.Invoke(abilityEnum.MemberToValue("ElectricShock"));
            }
            if ((speciality == EZombieSpeciality.BOSS_KUWAIT || speciality.IsFromBuakMap()) && Time.time - lastFlashbang > flashbangDelay && horizontalDistance > 4f && horizontalDistance < 1024f)
            {
                availableAbilityChoices_Add.Invoke(abilityEnum.MemberToValue("Flashbang"));
            }
        }
    }

    public static class CodeInstructionExtensions
    {
        public static void Replace(this CodeInstruction codeInstruction, OpCode opCode, object operand = null)
        {
            codeInstruction.opcode = opCode;
            codeInstruction.operand = operand;
        }

        public static bool DeepEquals(this CodeInstruction instruction, CodeInstruction other)
        {
            return instruction.opcode == other.opcode && instruction.operand == other.operand;
        }
    }
}
