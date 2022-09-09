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
        public PlayerData Player;
        public Team Team;
        public byte Amount;
        public double Price;

        public BuyItemEventArgs(ShopItem shopItem, PlayerData player, Team team, byte amount, double price)
        {
            ShopItem = shopItem ?? throw new ArgumentNullException(nameof(shopItem));
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Team = team ?? throw new ArgumentNullException(nameof(team));
            Amount = amount;
            Price = price;
        }
    }
}
