using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
