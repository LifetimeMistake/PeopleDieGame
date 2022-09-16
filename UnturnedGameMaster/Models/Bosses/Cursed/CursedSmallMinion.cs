using SDG.Unturned;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models.Bosses.Cursed
{
    public class CursedSmallMinion : IZombieModel
    {
        public string Name { get; } = "Lesser Spirit";
        public ushort Health { get; } = 100;
        public EZombieSpeciality Speciality { get; } = EZombieSpeciality.SPIRIT;
        public ZombieAbilities Abilities { get; } = ZombieAbilities.None;
        public byte HatId { get; } = 0;
        public byte ShirtId { get; } = 0;
        public byte PantsId { get; } = 0;
        public byte GearId { get; } = 0;
    }
}
