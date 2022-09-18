using HarmonyLib;
using SDG.Unturned;
using UnityEngine;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Reflection;

namespace UnturnedGameMaster.Models
{
    public class ManagedZombie : Zombie
    {
        private FieldRef<ushort> health;
        private FieldRef<ushort> maxHealth;

        private FieldRef<float> throwTime;
        private FieldRef<float> acidTime;
        private FieldRef<float> windTime;
        private FieldRef<float> fireTime;
        private FieldRef<float> chargeTime;
        private FieldRef<float> sparkTime;
        private FieldRef<float> boulderTime;
        private FieldRef<float> spitTime;

        private FieldRef<float> fireDamage;
        private FieldRef<EZombiePath> path; 

        private delegate void VoidDelegate();
        private VoidDelegate baseUpdate;

        public ZombieAbilities Abilities { get; set; }
        public ushort Health { get => health.Value; set => health.Value = value; }
        public ushort MaxHealth { get => maxHealth.Value; set => maxHealth.Value = value; }
        public float ThrowTime { get => throwTime.Value; set => throwTime.Value = value; }
        public float AcidTime { get => acidTime.Value; set => acidTime.Value = value; }
        public float StompTime { get => windTime.Value; set => windTime.Value = value; }
        public float FireTime { get => fireTime.Value; set => fireTime.Value = value; }
        public float ChargeTime { get => chargeTime.Value; set => chargeTime.Value = value; }
        public float SparkTime { get => sparkTime.Value; set => sparkTime.Value = value; }
        public float BoulderTime { get => boulderTime.Value; set => boulderTime.Value = value; }
        public float SpitTime { get => spitTime.Value; set => spitTime.Value = value; }
        public float FireDamage { get => fireDamage.Value; set => fireDamage.Value = value; }
        public EZombiePath Path { get => path.Value; set => path.Value = value; }

        public ManagedZombie()
        {
            baseUpdate = AccessTools.MethodDelegate<VoidDelegate>(AccessTools.Method(typeof(Zombie), "Update"), this);

            health = FieldRef.GetFieldRef<Zombie, ushort>(this, "health");
            maxHealth = FieldRef.GetFieldRef<Zombie, ushort>(this, "maxHealth");

            throwTime = FieldRef.GetFieldRef<Zombie, float>(this, "throwTime");
            acidTime = FieldRef.GetFieldRef<Zombie, float>(this, "acidTime");
            windTime = FieldRef.GetFieldRef<Zombie, float>(this, "windTime");
            fireTime = FieldRef.GetFieldRef<Zombie, float>(this, "fireTime");
            chargeTime = FieldRef.GetFieldRef<Zombie, float>(this, "chargeTime");
            sparkTime = FieldRef.GetFieldRef<Zombie, float>(this, "sparkTime");
            boulderTime = FieldRef.GetFieldRef<Zombie, float>(this, "boulderTime");
            spitTime = FieldRef.GetFieldRef<Zombie, float>(this, "spitTime");

            fireDamage = FieldRef.GetFieldRef<Zombie, float>(this, "fireDamage");
            path = FieldRef.GetFieldRef<Zombie, EZombiePath>(this, "path");
        }

        public bool CanUseAbility(ZombieAbilities zombieAbility)
        {
            return Abilities.HasFlag(zombieAbility);
        }

        public void AddAbilities(ZombieAbilities zombieAbility)
        {
            Abilities |= zombieAbility;
        }

        public void SetAbilities(ZombieAbilities zombieAbility)
        {
            Abilities = zombieAbility;
        }

        public void RemoveAbilities(ZombieAbilities zombieAbility)
        {
            Abilities &= ~zombieAbility;
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
            ManagedZombie managedZombie = zombie as ManagedZombie;
            Transform zombieTransform = ((MonoBehaviour)zombie).transform;
            VoidDelegate RandomizeBoulderThrowDelay = AccessTools.MethodDelegate<VoidDelegate>(AccessTools.Method(typeof(Zombie), "RandomizeBoulderThrowDelay"), zombie);
            if (player != null && ((managedZombie == null && (zombie.speciality == EZombieSpeciality.MEGA || zombie.speciality == EZombieSpeciality.BOSS_KUWAIT || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f))) || (managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Throw) && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > boulderThrowDelay)
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
            if (player != null && ((managedZombie == null && (zombie.speciality == EZombieSpeciality.ACID || zombie.speciality == EZombieSpeciality.BOSS_NUCLEAR || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f))) || (managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Spit) && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieSpit(zombie);
            }
            if (player != null && ((managedZombie == null && (zombie.speciality == EZombieSpeciality.BOSS_WIND || zombie.speciality == EZombieSpeciality.BOSS_ELVER_STOMPER || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f))) || (managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Stomp) && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay && (player.transform.position - zombieTransform.position).sqrMagnitude < 144f)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieStomp(zombie);
            }
            if (player != null && ((managedZombie == null && (zombie.speciality == EZombieSpeciality.BOSS_FIRE || zombie.speciality == EZombieSpeciality.BOSS_MAGMA || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f))) || (managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Breath) && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay && (player.transform.position - zombieTransform.position).sqrMagnitude < 529f)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieBreath(zombie);
            }
            if (player != null && ((managedZombie == null && (zombie.speciality == EZombieSpeciality.BOSS_ELECTRIC || (zombie.speciality == EZombieSpeciality.BOSS_ALL && UnityEngine.Random.value < 0.2f))) || (managedZombie != null && managedZombie.CanUseAbility(ZombieAbilities.Charge) && UnityEngine.Random.value < 0.2f)) && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay && (player.transform.position - zombieTransform.position).sqrMagnitude > 4f && (player.transform.position - zombieTransform.position).sqrMagnitude < 4096f)
            {
                lastSpecial = Time.time;
                specialUseDelay = UnityEngine.Random.Range(4f, 8f);
                seeker.canMove = false;
                ZombieManager.sendZombieCharge(zombie);
            }
        }
    }
}
