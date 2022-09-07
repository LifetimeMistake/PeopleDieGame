using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class BossArena
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool Conquered { get; set; }
        public IBoss BossModel { get; private set; }
        public VectorPAR BossSpawnPoint { get; set; }
        public VectorPAR RewardSpawnPoint { get; set; }
        public double ActivationDistance { get; private set; }
        public double DeactivationDistance { get; private set; }
        public double CompletionBounty { get; private set; }
        public double CompletionReward { get; private set; }

        public BossArena(int id, string name, bool conquered, IBoss bossModel, VectorPAR bossSpawnPoint, VectorPAR rewardSpawnPoint, double activationDistance, double deactivationDistance, double completionBounty, double completionReward)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Conquered = conquered;
            BossModel = bossModel ?? throw new ArgumentNullException(nameof(bossModel));
            BossSpawnPoint = bossSpawnPoint;
            RewardSpawnPoint = rewardSpawnPoint;
            ActivationDistance = activationDistance;
            DeactivationDistance = deactivationDistance;
            CompletionBounty = completionBounty;
            CompletionReward = completionReward;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public void SetBoss(IBoss model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            BossModel = model;
        }

        public void SetActivationDistance(double distance)
        {
            if (distance < 0)
                throw new ArgumentOutOfRangeException(nameof(distance));

            ActivationDistance = distance;
        }

        public void SetDeactivationDistance(double distance)
        {
            if (distance < 0)
                throw new ArgumentOutOfRangeException(nameof(distance));

            DeactivationDistance = distance;
        }

        public void SetCompletionReward(double reward)
        {
            if (reward < 0)
                throw new ArgumentOutOfRangeException(nameof(reward));

            CompletionReward = reward;
        }

        public void SetCompletionBounty(double bounty)
        {
            if (bounty < 0)
                throw new ArgumentOutOfRangeException(nameof(bounty));

            CompletionBounty = bounty;
        }
    }
}
