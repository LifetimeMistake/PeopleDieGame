using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Managers
{
    public class ZombiePoolManager : IDisposableManager
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private TimerManager timerManager { get; set; }
        private Dictionary<byte, ManagedZombie[]> managedZombiePools = new Dictionary<byte, ManagedZombie[]>();

        public void Init()
        {
            timerManager.Register(TryLoadPools, 60);
        }

        public void Dispose()
        {
            timerManager.Unregister(TryLoadPools);

            if (ZombieManager.regions == null)
            {
                // Cannot destroy the managed zombies because ????
                managedZombiePools.Clear();
                return;
            }

            foreach (KeyValuePair<byte, int> kvp in dataManager.GameData.ManagedZombiePools)
            {
                if (!managedZombiePools.ContainsKey(kvp.Key))
                    continue;

                foreach (ManagedZombie zombie in managedZombiePools[kvp.Key].Where(x => x != null))
                    DestroyZombieSlot(kvp.Key, zombie);

                managedZombiePools.Remove(kvp.Key);
            }
        }

        private void TryLoadPools()
        {
            if (ZombieManager.regions == null)
            {
                Debug.LogWarning("Attempted to load managed zombie pools but in-game ZombieManager was not ready yet (this message is harmless)");
                return;
            }

            foreach (KeyValuePair<byte, int> kvp in dataManager.GameData.ManagedZombiePools)
            {
                ManagedZombie[] zombieSlots = AllocateZombieSlots(kvp.Key, kvp.Value);
                if (zombieSlots == null)
                {
                    throw new Exception($"Failed to allocate zombie slots for bound {kvp.Key}");
                }
                managedZombiePools.Add(kvp.Key, zombieSlots);
            }

            Debug.Log("Loaded managed zombie pools!");
            timerManager.Unregister(TryLoadPools);
        }

        private ManagedZombie InitZombieSlot(byte boundId)
        {
            if (boundId > ZombieManager.regions.Length - 1)
            {
                return null; // region doesn't exist
            }

            ushort zombieId = (ushort)ZombieManager.regions[(int)boundId].zombies.Count;
            GameObject zombiePrefab = (GameObject)(StaticResourceRef<GameObject>)AccessTools.Field(typeof(ZombieManager), "dedicatedZombiePrefab").GetValue(null);
            GameObject zombieObject = UnityEngine.Object.Instantiate(zombiePrefab, Vector3.zero, Quaternion.identity);

            // replace the Zombie component with the ManagedZombie component
            GameObject.Destroy(zombieObject.GetComponent<Zombie>());

            zombieObject.name = "Zombie";
            ManagedZombie component = zombieObject.AddComponent<ManagedZombie>();
            component.id = zombieId;
            component.speciality = 0;
            component.bound = boundId;
            component.type = 0;
            component.shirt = 0;
            component.pants = 0;
            component.hat = 0;
            component.gear = 0;
            component.move = 0;
            component.idle = 0;
            component.isDead = true;
            component.init();
            ZombieManager.regions[boundId].zombies.Add(component);
            return component;
        }

        private ManagedZombie[] AllocateZombieSlots(byte boundId, int poolSize)
        {
            ManagedZombie[] zombieSlots = new ManagedZombie[poolSize];
            bool successful = true;

            for (int i = 0; i < poolSize; i++)
            {
                ManagedZombie zombie = InitZombieSlot(boundId);
                if (zombie == null)
                {
                    successful = false;
                    break;
                }

                zombieSlots[i] = zombie;
            }

            if (!successful)
            {
                // something went wrong idk why
                // but we gotta destroy the pool to prevent bad things from happening
                foreach (ManagedZombie zombie in zombieSlots.Where(x => x != null))
                    DestroyZombieSlot(boundId, zombie);

                return null;
            }

            return zombieSlots;
        }

        private bool DestroyZombieSlot(byte boundId, ManagedZombie slot)
        {
            if (boundId > ZombieManager.regions.Length - 1)
                return false; // region doesn't exist

            // kill zombie first to prevent desync
            if (!slot.isDead)
                ZombieManager.sendZombieDead(slot, Vector3.zero, ERagdollEffect.NONE);

            ZombieManager.tickingZombies.Remove(slot);
            ZombieManager.regions[(int)boundId].zombies.Remove(slot);
            GameObject.Destroy(slot.gameObject);
            return true;
        }

        public bool CreateZombiePool(byte boundId, int poolSize)
        {
            if (dataManager.GameData.ManagedZombiePools.ContainsKey(boundId))
                return false;

            if (poolSize < 0)
                throw new ArgumentOutOfRangeException(nameof(poolSize));

            ManagedZombie[] zombieSlots = AllocateZombieSlots(boundId, poolSize);
            if (zombieSlots == null)
                return false;

            managedZombiePools.Add(boundId, zombieSlots);
            dataManager.GameData.ManagedZombiePools.Add(boundId, poolSize);
            return true;
        }

        public bool ResizeZombiePool(byte boundId, int newPoolSize, bool force = false)
        {
            if (managedZombiePools.ContainsKey(boundId))
                return false;

            if (newPoolSize < 0)
                throw new ArgumentOutOfRangeException(nameof(newPoolSize));


            ManagedZombie[] zombieSlots = managedZombiePools[boundId];
            if (newPoolSize < zombieSlots.Length)
            {
                if (zombieSlots.Count(x => !x.isDead) > newPoolSize && !force)
                    return false;

                IEnumerable<ManagedZombie> markedForDestruction = zombieSlots.OrderByDescending(x => x.isDead).Take(zombieSlots.Length - newPoolSize);
                foreach (ManagedZombie zombie in markedForDestruction)
                    DestroyZombieSlot(boundId, zombie);

                managedZombiePools[boundId] = zombieSlots.Except(markedForDestruction).ToArray();
            }
            else
            {
                ManagedZombie[] newZombies = AllocateZombieSlots(boundId, newPoolSize - zombieSlots.Length);
                if (newZombies == null)
                    return false;

                managedZombiePools[boundId] = zombieSlots.Concat(newZombies).ToArray();
            }

            dataManager.GameData.ManagedZombiePools[boundId] = newPoolSize;
            return true;
        }

        public bool RemoveZombiePool(byte boundId)
        {
            if (!managedZombiePools.ContainsKey(boundId))
                return false;

            foreach (ManagedZombie zombie in managedZombiePools[boundId].Where(x => x != null))
                DestroyZombieSlot(boundId, zombie);

            managedZombiePools.Remove(boundId);
            dataManager.GameData.ManagedZombiePools.Remove(boundId);
            return true;
        }

        public ManagedZombie[] GetZombiePool(byte boundId)
        {
            if (!managedZombiePools.ContainsKey(boundId))
                return null;

            return managedZombiePools[boundId];
        }

        public int GetZombiePoolSize(byte boundId)
        {
            return GetZombiePool(boundId)?.Length ?? 0;
        }

        public bool ZombiePoolExists(byte boundId)
        {
            return managedZombiePools.ContainsKey(boundId);
        }

        public ManagedZombie SpawnZombie(byte boundId, byte type, byte speciality, byte shirt, byte pants, byte hat, byte gear, Vector3 position, float angle, bool force = false)
        {
            ManagedZombie[] zombiePool = GetZombiePool(boundId);
            if (zombiePool == null)
                return null;

            // check if there are any free zombie slots
            if (!zombiePool.Any(x => x.isDead) && !force)
                return null;

            ManagedZombie freeSlot = zombiePool.FirstOrDefault(x => x.isDead) ?? zombiePool.OrderBy(x => x.lastDead).FirstOrDefault();
            if (freeSlot == null) // no slots are available *AT ALL*
                return null;

            DestroyZombie(freeSlot);
            ZombieManager.sendZombieAlive(freeSlot, type, speciality, shirt, pants, hat, gear, position, MeasurementTool.angleToByte(angle));
            return freeSlot;
        }

        public ManagedZombie SpawnZombie(byte boundId, byte type, IZombieModel zombieModel, Vector3 position, float angle, bool force = false)
        {
            ManagedZombie managedZombie = SpawnZombie(boundId, type, (byte)zombieModel.Speciality, zombieModel.ShirtId, zombieModel.PantsId, zombieModel.HatId, zombieModel.GearId, position, angle, force);
            if (managedZombie == null)
                return null;

            managedZombie.Health = zombieModel.Health;
            managedZombie.MaxHealth = zombieModel.Health;
            managedZombie.SetAbilities(zombieModel.Abilities);

            return managedZombie;
        }

        public void DestroyZombie(ManagedZombie zombie)
        {
            if (!zombie.isDead)
            {
                ZombieManager.sendZombieDead(zombie, Vector3.zero, ERagdollEffect.NONE);
            }
        }
    }
}
