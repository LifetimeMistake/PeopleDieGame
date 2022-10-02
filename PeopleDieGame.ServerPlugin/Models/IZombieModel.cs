using SDG.Unturned;
using PeopleDieGame.ServerPlugin.Enums;

namespace PeopleDieGame.ServerPlugin.Models
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
