using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class BuyItemEventArgs : System.EventArgs
    {
        public ShopItem ShopItem;
        public PlayerData PlayerData;
        public Team Team;
        public byte Amount;
        public double Price;

        public BuyItemEventArgs(ShopItem shopItem, PlayerData playerData, Team team, byte amount, double price)
        {
            ShopItem = shopItem ?? throw new ArgumentNullException(nameof(shopItem));
            PlayerData = playerData ?? throw new ArgumentNullException(nameof(playerData));
            Team = team ?? throw new ArgumentNullException(nameof(team));
            Amount = amount;
            Price = price;
        }
    }
}
