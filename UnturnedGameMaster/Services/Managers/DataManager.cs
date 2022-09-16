using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Providers;

namespace UnturnedGameMaster.Services.Managers
{
    public class DataManager : IDisposableService
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
