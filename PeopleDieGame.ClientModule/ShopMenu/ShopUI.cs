using PeopleDieGame.ClientModule.ShopMenu.EventArgs;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ClientModule.ShopMenu
{
    public class ShopUI
    {
        private readonly int PADDING = 10;
        private Dictionary<ushort, float> shopItems = new Dictionary<ushort, float>();
        private Dictionary<ushort, ShopElement> itemDisplays = new Dictionary<ushort, ShopElement>();

        public bool Active;
        private float balance;

        private ISleekElement container;
        private ISleekElement headerContainer;
        private ISleekLabel titleLabel;
        private ISleekButton closeButton;
        private ISleekLabel balanceLabel;
        private ISleekElement contentContainer;
        private ISleekField searchBar;
        private ISleekScrollView itemView;

        public event EventHandler<RequestItemPurchaseEventArgs> OnRequestItemPurchase;

        public ShopUI()
        {
            container = Glazier.Get().CreateBox();
            container.sizeScale_X = 0.6f;
            container.sizeScale_Y = 0.6f;
            container.positionScale_X = 0.5f - (container.sizeScale_X / 2);
            container.positionScale_Y = 0.5f - (container.sizeScale_Y / 2);

            headerContainer = Glazier.Get().CreateFrame();
            headerContainer.sizeScale_X = 1f;
            headerContainer.sizeOffset_Y = 75;
            container.AddChild(headerContainer);

            titleLabel = Glazier.Get().CreateLabel();
            titleLabel.sizeOffset_X = 150;
            titleLabel.sizeScale_Y = 1f;
            titleLabel.positionScale_X = 0.5f;
            titleLabel.positionScale_Y = 0.5f - (titleLabel.sizeScale_Y / 2);
            titleLabel.positionOffset_X = -(titleLabel.sizeOffset_X / 2);
            titleLabel.text = "Sklep";
            titleLabel.fontSize = ESleekFontSize.Large;
            headerContainer.AddChild(titleLabel);

            closeButton = Glazier.Get().CreateButton();
            closeButton.sizeOffset_X = 150;
            closeButton.sizeScale_Y = 0.5f;
            closeButton.positionScale_X = 1f;
            closeButton.positionOffset_X = -closeButton.sizeOffset_X - PADDING;
            closeButton.positionOffset_Y = PADDING;
            closeButton.text = "Zamknij";
            closeButton.fontSize = ESleekFontSize.Large;
            closeButton.onClickedButton += CloseButton_onClickedButton;
            headerContainer.AddChild(closeButton);

            balanceLabel = Glazier.Get().CreateLabel();
            balanceLabel.positionOffset_X = PADDING;
            balanceLabel.positionOffset_Y = PADDING;
            balanceLabel.sizeOffset_X = 250;
            balanceLabel.sizeScale_Y = 1f;
            balanceLabel.sizeOffset_Y = -PADDING * 2;
            balanceLabel.fontSize = ESleekFontSize.Large;
            headerContainer.AddChild(balanceLabel);

            contentContainer = Glazier.Get().CreateFrame();
            contentContainer.sizeScale_X = 1f;
            contentContainer.sizeScale_Y = 1f;
            contentContainer.sizeOffset_Y = -headerContainer.sizeOffset_Y;
            contentContainer.positionOffset_Y = headerContainer.sizeOffset_Y;
            container.AddChild(contentContainer);

            searchBar = Glazier.Get().CreateStringField();
            searchBar.positionOffset_X = PADDING;
            searchBar.positionOffset_Y = PADDING;
            searchBar.sizeOffset_X = -PADDING * 2;
            searchBar.sizeScale_X = 1f;
            searchBar.sizeOffset_Y = 50;
            searchBar.hint = "Wyszukaj przedmioty...";
            searchBar.fontSize = ESleekFontSize.Large;
            searchBar.fontAlignment = UnityEngine.TextAnchor.MiddleLeft;
            searchBar.multiline = false;
            searchBar.onTyped += SearchBar_onTyped;
            contentContainer.AddChild(searchBar);

            itemView = Glazier.Get().CreateScrollView();
            itemView.positionOffset_X = PADDING;
            itemView.positionOffset_Y = PADDING * 2 + searchBar.sizeOffset_Y;
            itemView.sizeScale_X = 1f;
            itemView.sizeOffset_X = -PADDING * 2;
            itemView.sizeOffset_Y = -PADDING * 2 - searchBar.sizeOffset_Y;
            itemView.sizeScale_Y = 1f;
            itemView.scaleContentToWidth = true;
            itemView.scaleContentToHeight = true;
            contentContainer.AddChild(itemView);

            PlayerUI.container.AddChild(container);
            UpdateBalance(0f);
            Close();
        }

        private void CloseButton_onClickedButton(ISleekElement button)
        {
            Close();
        }

        private void SearchBar_onTyped(ISleekField field, string text)
        {
            if (!Active)
                return;

            UpdateFilter(text);
        }

        private void ClearView()
        {
            foreach (ShopElement element in itemDisplays.Values)
                element.OnBuyButtonPressed -= OnBuyButtonPressed;

            itemView.RemoveAllChildren();
            itemDisplays.Clear();
        }

        private void RenderView()
        {
            List<ShopElement> elements = itemDisplays.Values.Where(x => x.Active).ToList();
            int offset_Y = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                ShopElement element = elements[i];
                element.container.positionOffset_Y = offset_Y;

                if (i % 2 == 0)
                {
                    element.container.positionOffset_X = PADDING;
                    element.container.positionScale_X = 0f;
                }
                else
                {
                    element.container.positionOffset_X = PADDING;
                    element.container.positionScale_X = 0.5f;
                    offset_Y += element.container.sizeOffset_Y + PADDING;
                }
            }

            itemView.contentSizeOffset = new Vector2(0, offset_Y);
        }

        private void UpdateFilter(string filter = "")
        {
            foreach (ShopElement element in itemDisplays.Values)
            {
                element.Active = element.ItemAsset.FriendlyName.ToLowerInvariant().Contains(filter.ToLowerInvariant());
            }

            RenderView();
        }

        private void UpdateAvailability()
        {
            foreach (ShopElement element in itemDisplays.Values)
            {
                element.CanBuy = element.Price <= balance;
            }
        }

        private void RebuildView()
        {
            ClearView();
            foreach (KeyValuePair<ushort, float> kvp in shopItems)
            {
                ShopElement shopElement = new ShopElement(itemView, kvp.Key, kvp.Value);
                shopElement.OnBuyButtonPressed += OnBuyButtonPressed; ;
                itemDisplays.Add(kvp.Key, shopElement);
            }

            UpdateAvailability();
        }

        private void OnBuyButtonPressed(object sender, EventArgs.RequestItemPurchaseEventArgs e)
        {
            OnRequestItemPurchase?.Invoke(sender, e);
        }

        public void Open()
        {
            if (Active)
                return;

            PlayerUI.isLocked = true;
            PlayerLifeUI.close();
            searchBar.text = "";
            UpdateFilter("");
            container.isVisible = true;
            Active = true;
        }

        public void UpdateItems(Dictionary<ushort, float> items)
        {
            shopItems.Clear();
            foreach (KeyValuePair<ushort, float> item in items)
                shopItems.Add(item.Key, item.Value);

            RebuildView();

            if (Active)
                UpdateFilter(searchBar.text);
        }

        public void UpdateBalance(float teamBalance)
        {
            balance = teamBalance;
            balanceLabel.text = $"Stan konta: ${teamBalance}";
            UpdateAvailability();
        }

        public void Close()
        {
            PlayerLifeUI.open();
            PlayerUI.isLocked = false;
            container.isVisible = false;
            Active = false;
        }
    }
}
