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
        string Name { get; set; }
        double Health { get; set; }
        EZombieSpeciality Attributes { get; set; }
        byte HatId { get; set; }
        byte ShirtId { get; set; }
        byte PantsId { get; set; }
        byte GearId { get; set; }
    }
}
