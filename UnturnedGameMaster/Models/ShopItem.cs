using SDG.Unturned;
using System;

namespace UnturnedGameMaster.Models
{
    public class ShopItem
    {
        public ushort UnturnedItemId { get; private set; }
        public string Name { get { return GetName(); } }
        public string Description { get { return GetDescription(); } }
        public double Price { get; private set; }

        public ShopItem(ushort unturnedItemId, double price)
        {
            UnturnedItemId = unturnedItemId;
            Price = price;
        }

        public string GetName()
        {
            ItemAsset item = Assets.find(EAssetType.ITEM, UnturnedItemId) as ItemAsset;
            if (item == null)
            {
                return null;
            }

            return item.FriendlyName;
        }

        public string GetDescription()
        {
            ItemAsset item = Assets.find(EAssetType.ITEM, UnturnedItemId) as ItemAsset;
            if (item == null)
            {
                return null;
            }

            return item.itemDescription;
        }

        public void SetPrice(double amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            Price = amount;
        }
    }
}
