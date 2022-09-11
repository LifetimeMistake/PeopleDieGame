using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models.Bosses.Groundpounder
{
    public class GroundpounderSmallMinion : IZombieModel
    {
        public string Name { get; } = "Mr. Small";
        public ushort Health { get; } = 100;
        public EZombieSpeciality Attributes { get; } = EZombieSpeciality.NORMAL;
        public ZombieAbilities Abilities { get; } = ZombieAbilities.None;
        public byte HatId { get; } = 0;
        public byte ShirtId { get; } = 0;
        public byte PantsId { get; } = 0;
        public byte GearId { get; } = 0;
    }
}
