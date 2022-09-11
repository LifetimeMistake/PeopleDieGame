using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Enums
{
    [Flags]
    public enum ZombieAbilities
    {
        None = 0,
        Throw = 1,
        Spit = 2,
        Charge = 4,
        Stomp = 8,
        Breath = 16
    }
}
