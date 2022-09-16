using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.BossControllers
{
    public interface IBossController
    {
        bool StartFight();
        bool EndFight();
        bool IsBossDefeated();
        double GetBossHealthPercentage();
        bool Update();
        IZombieModel GetBossBase();
    }
}
