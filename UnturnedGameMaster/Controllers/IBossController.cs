using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Controllers
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
