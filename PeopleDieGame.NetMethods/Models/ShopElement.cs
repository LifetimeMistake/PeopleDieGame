using PeopleDieGame.NetMethods.Models.EventArgs;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models
{
    public class ShopElement
    {
        public ISleekElement container;
        private SleekItemIcon itemIcon;
        private ISleekLabel itemTitle;
        private ISleekLabel itemDescription;
        private ISleekLabel itemPrice;
        private ISleekButton buyButton;
        private ushort itemId;
        private float price;
        private ItemAsset asset;

        public event EventHandler<BuyButtonPressedEventArgs> OnBuyButtonPressed;

        public ushort ItemId { get => itemId; }
        public float Price
        {
            get
            { 
                return price; 
            }
            set
            {
                price = value;
                itemPrice.text = $"Cena: ${value}";
            }
        }

        public bool CanBuy
        {
            get
            {
                return buyButton.isVisible;
            }
            set
            {
                buyButton.isVisible = value;
                itemPrice.textColor = value ? Color.green : Color.red;
            }
        }

        public bool Active
        {
            get => container.isVisible;
            set => container.isVisible = value;
        }

        public ItemAsset ItemAsset
        {
            get => asset;
        }

        public ShopElement(ISleekElement root, ushort itemId, float price)
        {
            this.itemId = itemId;
            container = Glazier.Get().CreateBox();
            container.sizeScale_X = 0.5f;
            container.sizeOffset_X = -20;
            root.AddChild(container);

            buyButton = Glazier.Get().CreateButton();
            buyButton.sizeScale_X = 1f;
            buyButton.sizeScale_Y = 1f;
            buyButton.onClickedButton += BuyButton_onClickedButton;
            container.AddChild(buyButton);

            int num = 0;
            container.sizeOffset_Y = 60;
            asset = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;
            if (asset == null)
                throw new Exception($"Could not find item asset with iD {itemId}");

            itemIcon = new SleekItemIcon();
            itemIcon.positionOffset_X = 5;
            itemIcon.positionOffset_Y = 5;
            if (asset.size_y == 1)
            {
                itemIcon.sizeOffset_X = (asset.size_x * 100);
                itemIcon.sizeOffset_Y = (asset.size_y * 100);
            }
            else
            {
                itemIcon.sizeOffset_X = (asset.size_x * 50);
                itemIcon.sizeOffset_Y = (asset.size_y * 50);
            }

            num = itemIcon.sizeOffset_X;
            container.AddChild(itemIcon);
            itemIcon.Refresh(itemId, 100, asset.getState(false), asset, itemIcon.sizeOffset_X, itemIcon.sizeOffset_Y);

            container.sizeOffset_Y = itemIcon.sizeOffset_Y + 10;

            string displayName = asset.FriendlyName;
            if (!string.IsNullOrEmpty(displayName))
            {
                itemTitle = Glazier.Get().CreateLabel();
                itemTitle.positionOffset_X = num + 10;
                itemTitle.positionOffset_Y = 5;
                itemTitle.sizeOffset_X = -num - 15;
                itemTitle.sizeOffset_Y = 30;
                itemTitle.sizeScale_X = 1f;
                itemTitle.text = displayName;
                itemTitle.fontSize = ESleekFontSize.Medium;
                itemTitle.fontAlignment = TextAnchor.UpperLeft;
                itemTitle.textColor = ItemTool.getRarityColorUI(asset.rarity);
                itemTitle.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                container.AddChild(itemTitle);
            }
            string displayDesc = asset.itemDescription;
            if (!string.IsNullOrEmpty(displayDesc))
            {
                itemDescription = Glazier.Get().CreateLabel();
                itemDescription.positionOffset_X = num + 10;
                itemDescription.positionOffset_Y = 25;
                itemDescription.sizeOffset_X = -num - 15;
                itemDescription.sizeOffset_Y = -30;
                itemDescription.sizeScale_X = 1f;
                itemDescription.sizeScale_Y = 1f;
                itemDescription.fontAlignment = TextAnchor.UpperLeft;
                itemDescription.textColor = ESleekTint.RICH_TEXT_DEFAULT;
                itemDescription.enableRichText = true;
                itemDescription.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                itemDescription.text = displayDesc;
                container.AddChild(itemDescription);
            }

            itemPrice = Glazier.Get().CreateLabel();
            itemPrice.positionOffset_X = num + 10;
            itemPrice.positionOffset_Y = -35;
            itemPrice.positionScale_Y = 1f;
            itemPrice.sizeOffset_X = -num - 15;
            itemPrice.sizeOffset_Y = 30;
            itemPrice.sizeScale_X = 1f;
            itemPrice.fontAlignment = TextAnchor.LowerRight;
            container.AddChild(itemPrice);

            Price = price;
        }

        private void BuyButton_onClickedButton(ISleekElement button)
        {
            OnBuyButtonPressed?.Invoke(this, new BuyButtonPressedEventArgs(itemId, 1));
        }
    }
}
