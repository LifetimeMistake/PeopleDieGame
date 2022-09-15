using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models.Bosses.Cursed
{
    public class CursedBoss : IZombieModel
    {
        public string Name { get; } = "ono";
        public ushort Health { get; } = 6000;
        public EZombieSpeciality Speciality { get; } = EZombieSpeciality.NORMAL;
        public ZombieAbilities Abilities { get; } = ZombieAbilities.Stomp; // Starts only with melee and stomp
        public byte HatId { get; } = 0;
        public byte ShirtId { get; } = 0;
        public byte PantsId { get; } = 0;
        public byte GearId { get; } = 0;
    }
}
