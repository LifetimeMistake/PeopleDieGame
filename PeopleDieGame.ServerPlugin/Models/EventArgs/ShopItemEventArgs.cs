using System;

namespace PeopleDieGame.ServerPlugin.Models.EventArgs
{
    public class ShopItemEventArgs : System.EventArgs
    {
        public ShopItem ShopItem;

        public ShopItemEventArgs(ShopItem shopItem)
        {
            ShopItem = shopItem ?? throw new ArgumentNullException(nameof(shopItem));
        }
    }
}
