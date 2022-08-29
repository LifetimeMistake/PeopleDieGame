using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

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
