using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster
{
    public class ArenaBuilder
    {
        private string arenaName;
        private double activationDistance;
        private double deactivationDistance;
        private double completionBounty;
        private double completionReward;
        private VectorPAR bossSpawnpoint;
        private VectorPAR rewardSpawnpoint;
        private IBoss bossModel;

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

        public void SetBossSpawnPoint(VectorPAR spawnpoint)
        {
            bossSpawnpoint = spawnpoint;
        }

        public void SetRewardSpawnPoint(VectorPAR spawnpoint)
        {
            rewardSpawnpoint = spawnpoint;
        }

        public void SetBoss(IBoss model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            bossModel = model;
        }

        public BossArena ToArena(int arenaId)
        {
            return new BossArena(arenaId,
                arenaName,
                false,
                bossModel,
                bossSpawnpoint,
                rewardSpawnpoint,
                activationDistance,
                deactivationDistance,
                completionBounty,
                completionReward);
        }
    }
}
