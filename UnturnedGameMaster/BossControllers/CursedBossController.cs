using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.Bosses.Cursed;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.BossControllers
{
    public class CursedBossController : BossController<CursedBoss>
    {
        private CursedSmallMinion smallMinion;

        private BossFight fight;
        private BossArena arena;
        private CursedBoss bossModel;

        private bool spawnsMinions;
        private bool unlockedThrow;
        private bool unlockedBreath;
        private bool unlockedShock;

        private DateTime lastMinionSpawn;
        private int minionSpawnInterval;

        private ManagedZombie bossZombie;
        private List<ManagedZombie> minions = new List<ManagedZombie>();
        private ZombiePoolManager zombiePoolManager;

        public CursedBossController(BossFight bossFight)
        {
            this.fight = bossFight ?? throw new ArgumentNullException(nameof(bossFight));
            this.arena = bossFight.Arena;
            this.bossModel = (CursedBoss)bossFight.Arena.BossModel;
            this.zombiePoolManager = ServiceLocator.Instance.LocateService<ZombiePoolManager>();
            this.smallMinion = ServiceLocator.Instance.LocateService<CursedSmallMinion>();
        }

        public override bool StartFight()
        {
            Reset();

            byte boundId = arena.BoundId;
            bossZombie = zombiePoolManager.SpawnZombie(boundId, 0, bossModel, arena.BossSpawnPoint.Position, arena.BossSpawnPoint.Rotation, true);

            if (bossZombie == null)
                return false; // failed to spawn main boss

            ChatHelper.Say(fight.Participants, "Czujesz jak nagle otacza Cię chłód..");
            return true;
        }

        private void Reset()
        {
            spawnsMinions = false;
            unlockedThrow = false;
            unlockedBreath = false;
            unlockedShock = false;
            lastMinionSpawn = DateTime.MinValue;
            minionSpawnInterval = 0;
        }

        public override bool EndFight()
        {
            if (bossZombie != null && !bossZombie.isDead)
                zombiePoolManager.DestroyZombie(bossZombie);

            foreach (ManagedZombie managedZombie in minions.Where(x => !x.isDead))
                zombiePoolManager.DestroyZombie(managedZombie);

            ChatHelper.Say(fight.Participants, "Krzyki umarłych ustają...");
            return true;
        }

        public override IZombieModel GetBossBase()
        {
            return bossModel;
        }

        public override double GetBossHealthPercentage()
        {
            return bossZombie.GetHealth() / bossZombie.GetMaxHealth();
        }

        public override bool IsBossDefeated()
        {
            return bossZombie.isDead;
        }

        public override bool Update()
        {
            if (bossZombie == null || IsBossDefeated())
                return true; // nothing else to do

            double bossHealth = GetBossHealthPercentage();

            if (!spawnsMinions && bossHealth < 0.9)
            {
                spawnsMinions = true;
                minionSpawnInterval = 10;
            }

            if (spawnsMinions && (DateTime.Now - lastMinionSpawn).TotalSeconds > minionSpawnInterval)
            {
                SpawnMinion(smallMinion);
                lastMinionSpawn = DateTime.Now;
            }

            if (!unlockedThrow && bossHealth < 0.75)
            {
                unlockedThrow = true;
                bossZombie.AddAbilities(Enums.ZombieAbilities.Throw);
                minionSpawnInterval = 7;
                ChatHelper.Say(fight.Participants, $"{bossModel.Name} ewoluuje!");
            }

            if (!unlockedBreath && bossHealth < 0.5)
            {
                unlockedBreath = true;
                bossZombie.AddAbilities(Enums.ZombieAbilities.Breath);
                minionSpawnInterval = 5;
                ChatHelper.Say(fight.Participants, $"Powietrze wokół {bossModel.Name} zaczyna się nagrzewać...");
            }

            if (!unlockedShock && bossHealth < 0.3)
            {
                unlockedShock = true;
                bossZombie.AddAbilities(Enums.ZombieAbilities.Charge);
                minionSpawnInterval = 4;
                ChatHelper.Say(fight.Participants, $"{bossModel.Name} zaczyna uwalniać zgromadzoną energię...");
            }

            if (minionSpawnInterval > 3 && bossHealth < 0.15)
            {
                minionSpawnInterval = 3;
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
    }
}
