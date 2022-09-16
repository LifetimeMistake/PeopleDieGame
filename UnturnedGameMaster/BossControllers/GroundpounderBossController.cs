using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.Bosses;
using UnturnedGameMaster.Models.Bosses.Groundpounder;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.BossControllers
{
    public class GroundpounderBossController : BossController<GroundpounderBoss>
    {
        private GroundpounderLargeMinion largeMinion;
        private GroundpounderSmallMinion smallMinion;

        private BossFight fight;
        private BossArena arena;
        private GroundpounderBoss bossModel;

        private bool spawnsMinions;
        private bool usedLowerCooldown;
        private bool usedHeal;

        private float minionSpawnInterval;
        private DateTime lastMinionSpawn;
        private double lastMegaSpawnHealth;

        private ManagedZombie bossZombie;
        private List<ManagedZombie> minions = new List<ManagedZombie>();
        private ZombiePoolManager zombiePoolManager;

        public GroundpounderBossController(BossFight bossFight)
        {
            this.fight = bossFight ?? throw new ArgumentNullException(nameof(bossFight));
            this.arena = bossFight.Arena;
            this.bossModel = (GroundpounderBoss)bossFight.Arena.BossModel;
            this.zombiePoolManager = ServiceLocator.Instance.LocateService<ZombiePoolManager>();
            this.largeMinion = ServiceLocator.Instance.LocateService<GroundpounderLargeMinion>();
            this.smallMinion = ServiceLocator.Instance.LocateService<GroundpounderSmallMinion>();
        }

        public override bool StartFight()
        {
            Reset();

            byte boundId = arena.BoundId;
            bossZombie = zombiePoolManager.SpawnZombie(boundId, 0, bossModel, arena.BossSpawnPoint.Position, arena.BossSpawnPoint.Rotation, true);

            if (bossZombie == null)
                return false; // failed to spawn main boss

            ChatHelper.Say(fight.Participants, "Ziemia pod Twoimi stopami zaczyna się trząść...");
            return true;
        }

        private void Reset()
        {
            spawnsMinions = false;
            usedLowerCooldown = false;
            usedHeal = false;
            lastMinionSpawn = DateTime.MinValue;
            lastMegaSpawnHealth = 1f;
        }

        public override bool EndFight()
        {
            if (bossZombie != null && !bossZombie.isDead)
                zombiePoolManager.DestroyZombie(bossZombie);

            foreach (ManagedZombie managedZombie in minions.Where(x => !x.isDead))
                zombiePoolManager.DestroyZombie(bossZombie);

            ChatHelper.Say(fight.Participants, "Wibracje ziemi ustają...");
            return true;
        }

        public override bool IsBossDefeated()
        {
            return bossZombie.isDead;
        }

        public override double GetBossHealthPercentage()
        {
            return bossZombie.GetHealth() / bossZombie.GetMaxHealth();
        }

        public override bool Update()
        {
            // nothing more we can do :(
            if (IsBossDefeated() && usedHeal)
                return true;

            double bossHealth = GetBossHealthPercentage();

            if (!spawnsMinions && bossHealth < 0.9)
            {
                // Start spawning minions
                spawnsMinions = true;
                minionSpawnInterval = 10;
            }

            if (spawnsMinions && (DateTime.Now - lastMinionSpawn).TotalSeconds > minionSpawnInterval)
            {
                SpawnMinion(smallMinion);
                lastMinionSpawn = DateTime.Now;
            }

            if (!usedLowerCooldown && bossHealth < 0.5)
            {
                ChatHelper.Say(fight.Participants, $"{bossModel.Name} twardnieje...");
                bossZombie.ThrowTime *= 0.65f;
                bossZombie.StompTime *= 0.65f;
                usedLowerCooldown = true;
            }

            if (lastMegaSpawnHealth - bossHealth >= 0.2)
            {
                if (SpawnMinion(largeMinion))
                {
                    ChatHelper.Say(fight.Participants, "Kolejny przeciwnik wyłania się zza kamieni...");
                    lastMegaSpawnHealth = bossHealth;
                }
            }

            if (!usedHeal && bossHealth < 0.05)
            {
                ChatHelper.Say(fight.Participants, $"{bossModel.Name} przekracza swoje własne fizyczne ograniczenia, jego życie odnawia się do pełna!!");
                bossZombie.Health = bossModel.Health;
                Reset(); // its rewind time
                usedHeal = true;
            }

            return true;
        }

        private bool SpawnMinion(IZombieModel model)
        {
            Vector3 spawnPoint = GetMinionSpawnpoint();
            ManagedZombie managedZombie = zombiePoolManager.SpawnZombie(arena.BoundId, (byte)UnityEngine.Random.Range(0, 2), model, spawnPoint, 0f);
            if (managedZombie == null)
                return false;

            UnturnedPlayer closestPlayer = fight.Participants.OrderBy(x => Vector3.Distance(managedZombie.transform.position, x.Position)).FirstOrDefault();
            if (closestPlayer != null)
            {
                Vector3 direction = (closestPlayer.Position - managedZombie.transform.position).normalized;
                direction.y = 0f;
                managedZombie.transform.rotation = Quaternion.LookRotation(direction);
            }

            if (!minions.Contains(managedZombie))
                minions.Add(managedZombie);

            return true;
        }

        private Vector3 GetMinionSpawnpoint()
        {
            Vector3 bossPosition = bossZombie.transform.position;
            Vector3 randomPosition = bossPosition + new Vector3(UnityEngine.Random.Range(3, 10), 0, UnityEngine.Random.Range(3, 10));
            Ray ray = new Ray(randomPosition, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, RayMasks.GROUND))
                randomPosition = hit.point + new Vector3(0, 1, 0);

            return randomPosition;
        }

        public override IZombieModel GetBossBase()
        {
            return bossModel;
        }
    }
}
