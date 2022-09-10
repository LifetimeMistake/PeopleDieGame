using System;
using UnturnedGameMaster.Managers;

namespace UnturnedGameMaster.Providers
{
    public class LoadoutIdProvider
    {
        private DataManager dataManager;

        public LoadoutIdProvider(DataManager dataManager)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
        }

        public int GenerateId()
        {
            return dataManager.GameData.LastLoadoutId++;
        }
    }
}
