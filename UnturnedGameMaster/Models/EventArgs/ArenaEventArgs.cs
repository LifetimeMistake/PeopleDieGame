using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class ArenaEventArgs : System.EventArgs
    {
        public BossArena Arena;

        public ArenaEventArgs(BossArena arena)
        {
            Arena = arena ?? throw new ArgumentNullException(nameof(arena));
        }
    }
}
