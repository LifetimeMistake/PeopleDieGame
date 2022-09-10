using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Controllers
{
    public abstract class BossController<T> : IBossController where T : IBoss
    {
        public abstract bool EndFight();
        public abstract IBoss GetBossBase();
        public abstract double GetBossHealthPercentage();
        public abstract bool IsBossDefeated();
        public abstract bool StartFight();
        public abstract bool Update();
    }
}
