using PeopleDieGame.ServerPlugin.Models;

namespace PeopleDieGame.ServerPlugin.BossControllers
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
