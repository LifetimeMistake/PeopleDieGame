using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class DataManager : IDisposableManager
    {
        private IDatabaseProvider<GameData> databaseProvider;
        public GameData GameData { get { return databaseProvider.GetData(); } }

        public DataManager(IDatabaseProvider<GameData> databaseProvider)
        {
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        public void Init()
        { }

        public void Dispose()
        {
            // save config on unload
            CommitConfig();
        }

        public GameData GetConfig()
        {
            return databaseProvider.GetData();
        }

        public bool CommitConfig()
        {
            return databaseProvider.CommitData();
        }
    }
}
