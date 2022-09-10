using SDG.Unturned;

namespace UnturnedGameMaster.Models.Bosses
{
    public class TestBoss : IBoss
    {
        public string Name { get; } = "gamingu";
        public double Health { get; } = 1000;
        public EZombieSpeciality Attributes { get; } = EZombieSpeciality.MEGA;
        public byte HatId { get; } = 0;
        public byte ShirtId { get; } = 0;
        public byte PantsId { get; } = 0;
        public byte GearId { get; } = 0;
    }
}
