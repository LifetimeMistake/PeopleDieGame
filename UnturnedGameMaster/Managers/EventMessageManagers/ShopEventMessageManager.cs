using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Managers.EventMessageManagers
{
    public class ShopEventMessageManager : IManager
    {
        [InjectDependency]
        private ShopManager shopManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        public void Init()
        {
            shopManager.OnShopItemBought += ShopManager_OnShopItemBought;
        }

        private void ShopManager_OnShopItemBought(object sender, Models.EventArgs.BuyItemEventArgs e)
        {
            ChatHelper.Say(e.PlayerData, $"Zakupiono {e.ShopItem.Name} (x{e.Amount}) za ${e.Price}");
            foreach (PlayerData player in teamManager.GetTeamMembers(e.Team))
            {
                if (player == e.PlayerData)
                    continue;

                ChatHelper.Say(player, $"Gracz {e.PlayerData.Name} zakupił {e.ShopItem.Name} (x{e.Amount}) za ${e.Price}");
            }
        }
    }
}
