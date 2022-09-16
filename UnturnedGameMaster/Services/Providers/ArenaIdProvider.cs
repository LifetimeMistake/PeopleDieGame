using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Services.Providers
{
    public class ArenaIdProvider : IService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }

        public void Init()
        { }

        public int GenerateId()
        {
            return dataManager.GameData.LastArenaId++;
        }
    }
}
