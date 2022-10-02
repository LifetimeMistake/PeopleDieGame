using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Services.Providers
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
