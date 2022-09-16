using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Services.Providers
{
    public class TeamIdProvider : IService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }

        public void Init()
        { }

        public int GenerateId()
        {
            return dataManager.GameData.LastTeamId++;
        }
    }
}
