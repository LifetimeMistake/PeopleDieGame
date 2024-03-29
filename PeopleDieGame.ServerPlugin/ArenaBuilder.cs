﻿using SDG.Unturned;
using System;
using UnityEngine;
using PeopleDieGame.ServerPlugin.Models;

namespace PeopleDieGame.ServerPlugin
{
    public class ArenaBuilder
    {
        private string arenaName;
        private double activationDistance = 0;
        private double deactivationDistance = 0;
        private double completionBounty = 0;
        private double completionReward = 0;
        private Vector3 activationPoint;
        private VectorPAR bossSpawnpoint;
        private Vector3S rewardSpawnpoint;
        private IZombieModel bossModel;
        private byte boundId;
        private int zombiePoolSize;
        private int? rewardLoadoutId;

        public string ArenaName { get => arenaName; }
        public double ActivationDistance { get => activationDistance; }
        public double DeactivationDistance { get => deactivationDistance; }
        public double CompletionBounty { get => completionBounty; }
        public double CompletionReward { get => completionReward; }
        public Vector3 ActivationPoint { get => activationPoint; }
        public VectorPAR BossSpawnpoint { get => bossSpawnpoint; }
        public Vector3S RewardSpawnpoint { get => rewardSpawnpoint; }
        public IZombieModel BossModel { get => bossModel; }
        public byte BoundId { get => boundId; }
        public int ZombiePoolSize { get => zombiePoolSize; }
        public int? RewardLoadoutId { get => rewardLoadoutId; }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            arenaName = name;
        }

        public void SetActivationDistance(double distance)
        {
            if (distance < 0)
                throw new ArgumentOutOfRangeException(nameof(distance));

            activationDistance = distance;
        }

        public void SetDeactivationDistance(double distance)
        {
            if (distance < 0)
                throw new ArgumentOutOfRangeException(nameof(distance));

            deactivationDistance = distance;
        }

        public void SetCompletionReward(double reward)
        {
            if (reward < 0)
                throw new ArgumentOutOfRangeException(nameof(reward));

            completionReward = reward;
        }

        public void SetCompletionBounty(double bounty)
        {
            if (bounty < 0)
                throw new ArgumentOutOfRangeException(nameof(bounty));

            completionBounty = bounty;
        }

        public void SetActivationPoint(Vector3 point)
        {
            byte boundId;
            if (!LevelNavigation.tryGetBounds(point, out boundId))
                throw new ArgumentException("Point is outside of navigation grid bounds.");

            this.boundId = boundId;
            activationPoint = point;
        }

        public void SetBossSpawnPoint(VectorPAR spawnpoint)
        {
            byte boundId;
            if (!LevelNavigation.tryGetBounds(spawnpoint.Position, out boundId))
                throw new ArgumentException("Point is outside of navigation grid bounds.");

            this.boundId = boundId;
            bossSpawnpoint = spawnpoint;
        }

        public void SetRewardSpawnPoint(Vector3S spawnpoint)
        {
            rewardSpawnpoint = spawnpoint;
        }

        public void SetBoss(IZombieModel model)
        {
            bossModel = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void SetZombiePoolSize(int poolSize)
        {
            if (poolSize < 0)
                throw new ArgumentOutOfRangeException(nameof(poolSize));

            zombiePoolSize = poolSize;
        }

        public void SetRewardLoadout(Loadout loadout)
        {
            rewardLoadoutId = loadout?.Id ?? null;
        }

        public BossArena ToArena(int arenaId)
        {
            return new BossArena(arenaId,
                arenaName,
                false,
                bossModel,
                activationPoint,
                bossSpawnpoint,
                rewardSpawnpoint,
                activationDistance,
                deactivationDistance,
                completionBounty,
                completionReward,
                boundId,
                rewardLoadoutId);
        }
    }
}
