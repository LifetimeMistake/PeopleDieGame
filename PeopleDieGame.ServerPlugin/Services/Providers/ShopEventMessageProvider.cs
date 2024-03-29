﻿using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Helpers;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class ShopEventMessageProvider : IDisposableService
    {
        [InjectDependency]
        private ShopManager shopManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }

        public void Init()
        {
            shopManager.OnShopItemBought += ShopManager_OnShopItemBought;
        }

        public void Dispose()
        {
            shopManager.OnShopItemBought -= ShopManager_OnShopItemBought;
        }

        private void ShopManager_OnShopItemBought(object sender, Models.EventArgs.BuyItemEventArgs e)
        {
            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(e.Team))
            {
                if (player == e.Player)
                    continue;

                ChatHelper.Say(player, $"Gracz {e.Player.Name} zakupił {e.ShopItem.Name} (x{e.Amount}) za ${e.Price}");
            }
        }
    }
}
