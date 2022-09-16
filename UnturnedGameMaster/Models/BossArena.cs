using Newtonsoft.Json;
using SDG.Unturned;
using System;
using UnturnedGameMaster.Autofac;

namespace UnturnedGameMaster.Models
{
    public class BossArena
    {
        [JsonRequired]
        private Type bossType;
        [JsonIgnore]
        private IZombieModel zombieModel;
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool Conquered { get; set; }
        [JsonIgnore]
        public IZombieModel BossModel { get => GetBossModel(); }
        public Vector3S ActivationPoint { get; private set; }
        public VectorPAR BossSpawnPoint { get; private set; }
        public VectorPAR RewardSpawnPoint { get; private set; }
        public double ActivationDistance { get; private set; }
        public double DeactivationDistance { get; private set; }
        public double CompletionBounty { get; private set; }
        public double CompletionReward { get; private set; }
        public byte BoundId { get; private set; }

        public BossArena(int id, string name, bool conquered, IZombieModel bossModel, Vector3S activationPoint, VectorPAR bossSpawnPoint, VectorPAR rewardSpawnPoint, double activationDistance, double deactivationDistance, double completionBounty, double completionReward, byte boundId)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Conquered = conquered;
            SetBoss(bossModel);
            ActivationPoint = activationPoint;
            BossSpawnPoint = bossSpawnPoint;
            RewardSpawnPoint = rewardSpawnPoint;
            ActivationDistance = activationDistance;
            DeactivationDistance = deactivationDistance;
            CompletionBounty = completionBounty;
            CompletionReward = completionReward;
            BoundId = boundId;
        }

        [JsonConstructor]
        public BossArena(int id, string name, bool conquered, Type bossType, Vector3S activationPoint, VectorPAR bossSpawnPoint, VectorPAR rewardSpawnPoint, double activationDistance, double deactivationDistance, double completionBounty, double completionReward, byte boundId)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Conquered = conquered;
            this.bossType = bossType;
            ActivationPoint = activationPoint;
            BossSpawnPoint = bossSpawnPoint;
            RewardSpawnPoint = rewardSpawnPoint;
            ActivationDistance = activationDistance;
            DeactivationDistance = deactivationDistance;
            CompletionBounty = completionBounty;
            CompletionReward = completionReward;
            BoundId = boundId;
        }

        private IZombieModel GetBossModel()
        {
            if (zombieModel != null)
                return zombieModel;

            if (zombieModel == null && bossType != null)
                zombieModel = ServiceLocator.Instance.LocateService(bossType) as IZombieModel;

            return zombieModel;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public void SetBoss(IZombieModel model)
        {
            bossType = model.GetType();
            zombieModel = model;
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

        public void SetActivationPoint(Vector3S point)
        {
            byte boundId;
            if (!LevelNavigation.tryGetBounds(point, out boundId))
                throw new ArgumentException("Point is outside of navigation grid bounds.");

            BoundId = boundId;
            ActivationPoint = point;
        }

        public void SetBossSpawnPoint(VectorPAR spawnpoint)
        {
            byte boundId;
            if (!LevelNavigation.tryGetBounds(spawnpoint.Position, out boundId))
                throw new ArgumentException("Point is outside of navigation grid bounds.");

            BoundId = boundId;
            BossSpawnPoint = spawnpoint;
        }

        public void SetRewardSpawnPoint(VectorPAR spawnpoint)
        {
            RewardSpawnPoint = spawnpoint;
        }
    }
}
