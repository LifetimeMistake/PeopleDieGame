using System;
using UnturnedGameMaster.Managers;

namespace UnturnedGameMaster.Providers
{
    public class TeamIdProvider
    {
        private DataManager dataManager;

        public TeamIdProvider(DataManager dataManager)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
        }

        public int GenerateId()
        {
            return dataManager.GameData.LastTeamId++;
        }
    }
}
