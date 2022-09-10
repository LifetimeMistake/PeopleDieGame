using System;
using UnturnedGameMaster.Controllers;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class BossFight
    {
        public BossArena Arena { get; set; }
        public IBossController FightController { get; set; }
        public Team DominantTeam { get; set; }
        public BossFightState State { get; set; }

        public BossFight(BossArena arena, IBossController fightController, Team dominantTeam, BossFightState state)
        {
            Arena = arena ?? throw new ArgumentNullException(nameof(arena));
            FightController = fightController ?? throw new ArgumentNullException(nameof(fightController));
            DominantTeam = dominantTeam ?? throw new ArgumentNullException(nameof(dominantTeam));
            State = state;
        }
    }
}
