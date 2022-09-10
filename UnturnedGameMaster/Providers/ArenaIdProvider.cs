using System;
using UnturnedGameMaster.Managers;

namespace UnturnedGameMaster.Providers
{
    public class ArenaIdProvider
    {
        private DataManager dataManager;

        public ArenaIdProvider(DataManager dataManager)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
        }

        public int GenerateId()
        {
            return dataManager.GameData.LastArenaId++;
        }
    }
}
