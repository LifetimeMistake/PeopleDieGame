using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class Boss
    {
        string Name { get; set; }
        double Health { get; set; }
        EZombieSpeciality Attributes { get; set; }
        byte HatId { get; set; }
        byte ShirtId { get; set; }
        byte PantsId { get; set; }
        byte GearId { get; set; }

        public Boss(string name, double health, EZombieSpeciality attributes, byte hatId, byte shirtId, byte pantsId, byte gearId)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Health = health;
            Attributes = attributes;
            HatId = hatId;
            ShirtId = shirtId;
            PantsId = pantsId;
            GearId = gearId;
        }
    }
}
