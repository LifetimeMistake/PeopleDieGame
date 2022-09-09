using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Controllers;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class BossFight
    {
        public BossArena Arena { get; set; }
        public IBossController FightController { get; set; }
        public Team AttackingTeam { get; set; }
        public BossFightState State { get; set; }

        public BossFight(BossArena arena, IBossController fightController, Team attackingTeam, BossFightState state)
        {
            Arena = arena ?? throw new ArgumentNullException(nameof(arena));
            FightController = fightController ?? throw new ArgumentNullException(nameof(fightController));
            AttackingTeam = attackingTeam ?? throw new ArgumentNullException(nameof(attackingTeam));
            State = state;
        }
    }
}
