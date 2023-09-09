using PeopleDieGame.ClientModule.Autofac;
using PeopleDieGame.ClientModule.Models;
using PeopleDieGame.NetMethods.RPCs;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ClientModule.ShopMenu
{
    public class ShopMenuService : IService, IDisposable
    {
        [InjectDependency]
        private ClientDataService clientDataService { get; set; }

        private ShopUI shopUI;
        
        public void Init()
        {
            shopUI = new ShopUI();
            shopUI.OnRequestItemPurchase += ShopUI_OnRequestItemPurchase;
            ShopRPC.OnUpdateShopItems += ShopRPC_OnUpdateShopItems;
            clientDataService.OnTeamInfoUpdated += ClientDataService_OnTeamInfoUpdated;
        }

        public void Dispose()
        {
            if (shopUI.Active)
                shopUI.Close();

            shopUI.OnRequestItemPurchase -= ShopUI_OnRequestItemPurchase;
            shopUI = null;
            ShopRPC.OnUpdateShopItems -= ShopRPC_OnUpdateShopItems;
            clientDataService.OnTeamInfoUpdated -= ClientDataService_OnTeamInfoUpdated;
        }

        public void OpenShop()
        {
            if (!clientDataService.ClientData.PlayerInfo.HasValue)
                throw new Exception("ClientModule has not received PlayerData yet!");

            if (!clientDataService.ClientData.TeamInfo.HasValue)
                throw new Exception("Player does not belong to a team!");

            if (PlayerUI.isLocked)
                return;

            shopUI.Open();
        }

        public void CloseShop()
        {
            shopUI.Close();
        }

        public void UpdateBalance(float teamBalance)
        {
            shopUI.UpdateBalance(teamBalance);
        }

        private void ShopUI_OnRequestItemPurchase(object sender, EventArgs.RequestItemPurchaseEventArgs e)
        {
            ShopRPC.RequestItemPurchase(e.ItemId, e.Amount);
        }

        private void ShopRPC_OnUpdateShopItems(object sender, NetMethods.Models.EventArgs.UpdateShopItemsEventArgs e)
        {
            shopUI.UpdateItems(e.Items);
        }

        private void ClientDataService_OnTeamInfoUpdated(object sender, System.EventArgs e)
        {
            if (!clientDataService.ClientData.TeamInfo.HasValue)
                return;

            float teamBalance = clientDataService.ClientData.TeamInfo.Value.BankBalance;
            UpdateBalance(teamBalance);
        }
    }
}