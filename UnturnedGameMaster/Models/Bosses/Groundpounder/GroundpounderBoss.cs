using SDG.Unturned;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models.Bosses
{
    public class GroundpounderBoss : IZombieModel
    {
        public string Name { get; } = "gamingu";
        public ushort Health { get; } = 3000;
        public EZombieSpeciality Attributes { get; } = EZombieSpeciality.BOSS_WIND;
        public ZombieAbilities Abilities { get; } = ZombieAbilities.Throw | ZombieAbilities.Stomp;
        public byte HatId { get; } = 0;
        public byte ShirtId { get; } = 0;
        public byte PantsId { get; } = 0;
        public byte GearId { get; } = 0;
    }
}
