using System;

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
