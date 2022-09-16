using SDG.Unturned;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models.Bosses.Flamethrower
{
    public class FlamethrowerBoss : IZombieModel
    {
        public string Name { get; } = "oh that's hot";
        public ushort Health { get; } = 4500;
        public EZombieSpeciality Speciality { get; } = EZombieSpeciality.BOSS_FIRE;
        public ZombieAbilities Abilities { get; } = ZombieAbilities.Breath;
        public byte HatId { get; } = 0;
        public byte ShirtId { get; } = 0;
        public byte PantsId { get; } = 0;
        public byte GearId { get; } = 0;
    }
}
