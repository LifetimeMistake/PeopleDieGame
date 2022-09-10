using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.Bosses;

namespace UnturnedGameMaster.Controllers
{
    public class TestBossController : BossController<TestBoss>
    {
        public override bool StartFight()
        {
            ChatHelper.Say("fight started");
            return true;
        }

        public override bool EndFight()
        {
            ChatHelper.Say("fight stopped");
            return true;
        }

        public override bool IsBossDefeated()
        {
            return false;
        }

        public override double GetBossHealthPercentage()
        {
            return 1;
        }

        public override bool Update()
        {
            return true;
        }

        public override IBoss GetBossBase()
        {
            return null;
        }
    }
}
