using SDG.Unturned;
using System;

namespace PeopleDieGame.ServerPlugin.Models
{
    public class ShopItem
    {
        public ushort UnturnedItemId { get; private set; }
        public string Name { get { return GetName(); } }
        public string Description { get { return GetDescription(); } }
        public float Price { get; private set; }

        public ShopItem(ushort unturnedItemId, float price)
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

        public void SetPrice(float amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            Price = amount;
        }
    }
}
