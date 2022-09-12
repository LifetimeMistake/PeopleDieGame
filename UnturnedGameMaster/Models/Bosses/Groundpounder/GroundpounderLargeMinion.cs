using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models.Bosses.Groundpounder
{
    public class GroundpounderLargeMinion : IZombieModel
    {
        public string Name { get; } = "Sir Large";
        public ushort Health { get; } = 1000;
        public EZombieSpeciality Speciality { get; } = EZombieSpeciality.MEGA;
        public ZombieAbilities Abilities { get; } = ZombieAbilities.Throw | ZombieAbilities.Stomp;
        public byte HatId { get; } = 0;
        public byte ShirtId { get; } = 0;
        public byte PantsId { get; } = 0;
        public byte GearId { get; } = 0;
    }
}
