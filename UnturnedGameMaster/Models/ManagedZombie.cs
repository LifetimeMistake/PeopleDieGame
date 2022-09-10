using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class ManagedZombie : Zombie
    {
        private delegate void VoidDelegate();
        private VoidDelegate baseUpdate;
        public ZombieAbilities Abilities { get; set; }
        public float Health { get { return GetHealth(); } set { SetHealth(value); } }
        public float MaxHealth { get { return GetMaxHealth(); } set { SetMaxHealth(value); } }

        public ManagedZombie()
        {
            baseUpdate = AccessTools.MethodDelegate<VoidDelegate>(AccessTools.Method(typeof(Zombie), "Update"), this);
        }

        public new float GetHealth()
        {
            return base.GetHealth();
        }

        public new float GetMaxHealth()
        {
            return base.GetMaxHealth();
        }

        public void SetHealth(float health)
        {
            FieldInfo field = AccessTools.Field(typeof(Zombie), "health");
            field.SetValue(this, (ushort)health);
        }

        public void SetMaxHealth(float maxHealth)
        {
            FieldInfo field = AccessTools.Field(typeof(Zombie), "maxHealth");
            field.SetValue(this, (ushort)maxHealth);
        }

        private void Update()
        {
            // Passthrough unity event
            baseUpdate();
        }

        public static void UpdateAttacks(Zombie zombie, float targetDistance, ref float boulderThrowDelay, ref bool isThrowRelocating, 
            ref float lastAttack, ref float lastRelocate, ref float lastStartle, ref float lastSpecial, ref Player player, ref AIPath seeker, ref float specialStartleDelay,
            ref float specialUseDelay, ref float specialAttackDelay)
        {
            Transform zombieTransform = ((MonoBehaviour)zombie).transform;
            VoidDelegate RandomizeBoulderThrowDelay = AccessTools.MethodDelegate<VoidDelegate>(AccessTools.Method(typeof(Zombie), "RandomizeBoulderThrowDelay"), zombie);
            if (player != null && (zombie.speciality == EZombieSpeciality.MEGA || zombie.speciality == EZombieSpeciality.BOSS_KUWAIT || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > boulderThrowDelay)
            {
                if (targetDistance < 20f)
                {
                    if (isThrowRelocating)
                    {
                        if (Time.time - lastRelocate > 1.5f)
                        {
                            isThrowRelocating = false;
                            lastSpecial = Time.time;
                            RandomizeBoulderThrowDelay();
                        }
                    }
                    else
                    {
                        isThrowRelocating = true;
                        lastRelocate = Time.time;
                    }
                }
                else
                {
                    isThrowRelocating = false;
                    lastSpecial = Time.time;
                    RandomizeBoulderThrowDelay();
                    seeker.canMove = false;
                    ZombieManager.sendZombieThrow(zombie);
                }
            }
            else
            {
                isThrowRelocating = false;
            }
            if (player != null && (zombie.speciality == EZombieSpeciality.ACID || zombie.speciality == EZombieSpeciality.BOSS_NUCLEAR || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieSpit(zombie);
            }
            if (player != null && (zombie.speciality == EZombieSpeciality.BOSS_WIND || zombie.speciality == EZombieSpeciality.BOSS_ELVER_STOMPER || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay && (player.transform.position - zombieTransform.position).sqrMagnitude < 144f)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieStomp(zombie);
            }
            if (player != null && (zombie.speciality == EZombieSpeciality.BOSS_FIRE || zombie.speciality == EZombieSpeciality.BOSS_MAGMA || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay && (player.transform.position - zombieTransform.position).sqrMagnitude < 529f)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieBreath(zombie);
            }
            if (player != null && (zombie.speciality == EZombieSpeciality.BOSS_ELECTRIC || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay && (player.transform.position - zombieTransform.position).sqrMagnitude > 4f && (player.transform.position - zombieTransform.position).sqrMagnitude < 4096f)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieCharge(zombie);
            }
        }
    }
}
