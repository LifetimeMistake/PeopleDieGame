using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

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
