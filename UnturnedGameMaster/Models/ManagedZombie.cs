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
        private delegate void UpdateDelegate();
        private UpdateDelegate baseUpdate;
        public ZombieAbilities Abilities { get; set; }
        public float Health { get { return GetHealth(); } set { SetHealth(value); } }
        public float MaxHealth { get { return GetMaxHealth(); } set { SetMaxHealth(value); } }

        public ManagedZombie()
        {
            baseUpdate = AccessTools.MethodDelegate<UpdateDelegate>(AccessTools.Method(typeof(Zombie), "Update"), this);
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
    }
}
