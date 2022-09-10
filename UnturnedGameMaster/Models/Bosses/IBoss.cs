using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public interface IBoss
    {
        string Name { get; }
        double Health { get; }
        EZombieSpeciality Attributes { get; }
        byte HatId { get; }
        byte ShirtId { get; }
        byte PantsId { get; }
        byte GearId { get; }
    }
}
