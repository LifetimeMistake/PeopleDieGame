using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class ArenaManager : IDisposableManager
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private ArenaIdProvider arenaIdProvider { get; set; }

        public event EventHandler<ArenaEventArgs> OnArenaCreated;
        public event EventHandler<ArenaEventArgs> OnArenaRemoved;
        public event EventHandler<ArenaEventArgs> OnArenaActivated;
        public event EventHandler<ArenaEventArgs> OnArenaDeactivated;
        public event EventHandler<ArenaEventArgs> OnArenaCompleted;
        public event EventHandler<ArenaEventArgs> OnArenaFailed;

        public void Init()
        { }

        public void Dispose()
        { }

        public BossArena CreateArena(ArenaBuilder arenaBuilder)
        {
            if (arenaBuilder == null)
                throw new ArgumentNullException(nameof(arenaBuilder));

            int arenaId = arenaIdProvider.GenerateId();
            BossArena arena = arenaBuilder.ToArena(arenaId);

            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            arenas.Add(arenaId, arena);
            OnArenaCreated?.Invoke(this, new ArenaEventArgs(arena));
            return arena;
        }

        public bool UpdateArena(ArenaBuilder arenaBuilder, int arenaId)
        {
            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            if (!arenas.ContainsKey(arenaId))
                return false;

            BossArena oldArena = arenas[arenaId];
            BossArena newArena = arenaBuilder.ToArena(oldArena.Id);
            arenas[arenaId] = newArena;
            return true;
        }

        public bool DeleteArena(int id)
        {
            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            BossArena arena = GetArena(id);
            if (arena == null)
                return false;

            // TODO: Make sure all ongoing fights are killed prior to deletion.

            arenas.Remove(id);
            OnArenaRemoved?.Invoke(this, new ArenaEventArgs(arena));
            return true;
        }

        public BossArena GetArena(int id)
        {
            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            if (!arenas.ContainsKey(id))
                return null;

            return arenas[id];
        }

        public BossArena GetArenaByName(string name, bool exactMatch = true)
        {
            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            if (exactMatch)
                return arenas.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return arenas.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }
    }
}
