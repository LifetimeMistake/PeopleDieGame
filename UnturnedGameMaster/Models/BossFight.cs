using JetBrains.Annotations;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnturnedGameMaster.Controllers;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class BossFight
    {
        public BossArena Arena { get; set; }
        public IBossController FightController { get; set; }
        public Team DominantTeam { get; set; }
        public List<UnturnedPlayer> Participants { get; set; }
        public BossFightState State { get; set; }

        public BossFight(BossArena arena, IBossController fightController, Team dominantTeam, List<UnturnedPlayer> participants, BossFightState state)
        {
            Arena = arena ?? throw new ArgumentNullException(nameof(arena));
            FightController = fightController;
            DominantTeam = dominantTeam ?? throw new ArgumentNullException(nameof(dominantTeam));
            Participants = participants ?? throw new ArgumentNullException(nameof(participants));
            State = state;
        }

        public BossFight(BossArena arena, IBossController fightController, Team dominantTeam, BossFightState state) : this(arena, fightController, dominantTeam, new List<UnturnedPlayer>(), state)
        { }
    }
}
