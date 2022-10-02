using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.Bosses.Flamethrower;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.BossControllers
{
    public class FlamethrowerBossController : BossController<FlamethrowerBoss>
    {
        private FlamethrowerNormalMinion regularMinion;
        private FlamethrowerBurningMinion burningMinion;

        private BossFight fight;
        private BossArena arena;
        private FlamethrowerBoss bossModel;

        private bool spawnsMinions;
        private bool spawnsUpgradedMinions;
        private bool usedReduceCooldownOnce;
        private bool usedReduceCooldownTwice;

        private DateTime lastMinionSpawn;
        private int minionSpawnInterval;

        private ManagedZombie bossZombie;
        private List<ManagedZombie> minions = new List<ManagedZombie>();
        private ZombiePoolManager zombiePoolManager;

        public FlamethrowerBossController(BossFight bossFight)
        {
            this.fight = bossFight ?? throw new ArgumentNullException(nameof(bossFight));
            this.arena = bossFight.Arena;
            this.bossModel = (FlamethrowerBoss)bossFight.Arena.BossModel;
            this.zombiePoolManager = ServiceLocator.Instance.LocateService<ZombiePoolManager>();
            this.regularMinion = ServiceLocator.Instance.LocateService<FlamethrowerNormalMinion>();
            this.burningMinion = ServiceLocator.Instance.LocateService<FlamethrowerBurningMinion>();
        }

        public override bool StartFight()
        {
            Reset();

            byte boundId = arena.BoundId;
            bossZombie = zombiePoolManager.SpawnZombie(boundId, 0, bossModel, arena.BossSpawnPoint.Position, arena.BossSpawnPoint.Rotation, true);

            if (bossZombie == null)
                return false; // failed to spawn main boss

            if (UnityEngine.Random.Range(0, 1) < 0.5)
            {
                bossZombie.PathOverride = EZombiePath.LEFT_FLANK;
            }
            else
            {
                bossZombie.PathOverride = EZombiePath.RIGHT_FLANK;
            }

            ChatHelper.Say(fight.Participants, "Powietrze wokół Ciebie robi się bardzo gorące (czujesz też zapach BBQ)..");
            return true;
        }

        private void Reset()
        {
            spawnsMinions = false;
            spawnsUpgradedMinions = false;
            usedReduceCooldownOnce = false;
            usedReduceCooldownTwice = false;
            lastMinionSpawn = DateTime.MinValue;
            minionSpawnInterval = 0;
        }

        public override bool EndFight()
        {
            if (bossZombie != null && !bossZombie.isDead)
                zombiePoolManager.DestroyZombie(bossZombie);

            foreach (ManagedZombie managedZombie in minions.Where(x => !x.isDead))
                zombiePoolManager.DestroyZombie(managedZombie);

            ChatHelper.Say(fight.Participants, "Płomienie gasną...");
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

            if (!spawnsMinions)
            {
                spawnsMinions = true;
                minionSpawnInterval = 7;
            }

            if (spawnsMinions && (DateTime.Now - lastMinionSpawn).TotalSeconds > minionSpawnInterval)
            {
                IZombieModel minionModel = (spawnsUpgradedMinions) ? (IZombieModel)burningMinion : (IZombieModel)regularMinion;
                SpawnMinion(minionModel);
                lastMinionSpawn = DateTime.Now;
            }

            if (minionSpawnInterval > 6 && bossHealth < 0.65)
            {
                minionSpawnInterval = 6;
            }
            else if (minionSpawnInterval > 5 && bossHealth < 0.55)
            {
                minionSpawnInterval = 5;
            }
            else if (minionSpawnInterval > 4 && bossHealth < 0.4)
            {
                minionSpawnInterval = 4;
            }
            else if (minionSpawnInterval > 3 && bossHealth < 0.25)
            {
                minionSpawnInterval = 3;
            }
            else if (minionSpawnInterval > 2 && bossHealth < 0.15)
            {
                minionSpawnInterval = 2;
            }
            else if (minionSpawnInterval > 1 && bossHealth < 0.07)
            {
                minionSpawnInterval = 1;
            }

            if (!spawnsUpgradedMinions && bossHealth < 0.75)
            {
                spawnsUpgradedMinions = true;
                ChatHelper.Say(fight.Participants, $"Przyzwane stworzenia {bossModel.Name}'a zaczynają się topić!");
            }

            if ((!usedReduceCooldownOnce && bossHealth < 0.75) || (!usedReduceCooldownTwice && bossHealth < 0.5))
            {
                if (!usedReduceCooldownOnce)
                    usedReduceCooldownOnce = true;
                else
                    usedReduceCooldownTwice = true;

                bossZombie.FireTime *= 0.75f;
                ChatHelper.Say(fight.Participants, $"{bossModel.Name} nagrzewa się jeszcze bardziej!");
            }

            if (minionSpawnInterval != 3 && bossHealth < 0.15)
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
