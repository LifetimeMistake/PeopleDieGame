using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        IBoss GetBossBase();
    }
}
