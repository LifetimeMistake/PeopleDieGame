using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.Bosses
{
    public class TestBoss : IBoss
    {
        public string Name { get; } = "gamingu";
        public double Health { get; } = 1000;
        public EZombieSpeciality Attributes { get; } = EZombieSpeciality.MEGA | EZombieSpeciality.BOSS_FIRE;
        public byte HatId { get; }
        public byte ShirtId { get; }
        public byte PantsId { get; }
        public byte GearId { get; }

        public TestBoss(byte hatId, byte shirtId, byte pantsId, byte gearId)
        {
            HatId = hatId;
            ShirtId = shirtId;
            PantsId = pantsId;
            GearId = gearId;
        }
    }
}
