using SDG.Unturned;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public interface IZombieModel
    {
        string Name { get; }
        ushort Health { get; }
        EZombieSpeciality Speciality { get; }
        ZombieAbilities Abilities { get; }
        byte HatId { get; }
        byte ShirtId { get; }
        byte PantsId { get; }
        byte GearId { get; }
    }
}
