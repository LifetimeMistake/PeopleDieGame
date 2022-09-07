using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
