using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Services.Providers
{
    public class LoadoutIdProvider : IService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }

        public void Init()
        { }

        public int GenerateId()
        {
            return dataManager.GameData.LastLoadoutId++;
        }
    }
}
