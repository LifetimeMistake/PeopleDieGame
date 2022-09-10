using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class DataManager : IDisposableManager
    {
        [InjectDependency]
        private IDatabaseProvider<GameData> databaseProvider { get; set; }
        public GameData GameData { get { return databaseProvider.GetData(); } }

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
