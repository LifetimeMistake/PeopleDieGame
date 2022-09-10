using System;

namespace UnturnedGameMaster.Models.EventArgs
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
