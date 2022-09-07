using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Controllers;

namespace UnturnedGameMaster.Models
{
    public class ArenaBossFight
    {
        public BossArena Arena { get; set; }
        public IBossController FightController { get; set; }
        public Team AttackingTeam { get; set; }

        public ArenaBossFight(BossArena arena, IBossController fightController, Team attackingTeam)
        {
            Arena = arena ?? throw new ArgumentNullException(nameof(arena));
            FightController = fightController ?? throw new ArgumentNullException(nameof(fightController));
            AttackingTeam = attackingTeam ?? throw new ArgumentNullException(nameof(attackingTeam));
        }
    }
}
