using HarmonyLib;
using SDG.Unturned;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.Reflection;

namespace PeopleDieGame.ServerPlugin.Models
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
        private FieldRef<ZombieRegion> region;

        private delegate void VoidDelegate();
        private VoidDelegate baseUpdate;
        private EZombiePath? pathOverride;

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
        public EZombiePath? PathOverride { get => pathOverride; set => SetPathOverride(value); }
        public ZombieRegion zombieRegion { get => region.Value; set => region.Value = value; }
        public bool AIEnabled { get; set; }

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
            region = FieldRef.GetFieldRef<Zombie, ZombieRegion>(this, "zombieRegion");

            Reset();
        }

        public void Reset()
        {
            PathOverride = null;
            AIEnabled = true;
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
            if (!AIEnabled)
                return;

            // Passthrough unity event
            baseUpdate();
        }

        private void SetPathOverride(EZombiePath? path)
        {
            pathOverride = path;
            if (path.HasValue)
                Path = path.Value;
        }
    }
}
