using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Controllers
{
    public abstract class BossController<T> : IBossController where T : IZombieModel
    {
        public abstract bool EndFight();
        public abstract IZombieModel GetBossBase();
        public abstract double GetBossHealthPercentage();
        public abstract bool IsBossDefeated();
        public abstract bool StartFight();
        public abstract bool Update();
    }
}
