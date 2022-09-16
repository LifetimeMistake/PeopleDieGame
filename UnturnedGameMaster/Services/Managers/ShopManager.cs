using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;

namespace UnturnedGameMaster.Services.Managers
{
    internal class ShopManager : IService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }

        public event EventHandler<ShopItemEventArgs> OnShopItemAdded;
        public event EventHandler<ShopItemEventArgs> OnShopItemRemoved;
        public event EventHandler<BuyItemEventArgs> OnShopItemBought;
        public event EventHandler<ShopItemEventArgs> OnShopItemPriceChanged;
        public void Init()
        { }

        public ShopItem AddItem(ushort unturnedItemId, double price)
        {
            Dictionary<ushort, ShopItem> shopItems = dataManager.GameData.ShopItems;
            ItemAsset item = Assets.find(EAssetType.ITEM, unturnedItemId) as ItemAsset;

            if (item == null)
            {
                return null;
            }

            ShopItem shopItem = new ShopItem(unturnedItemId, price);
            shopItems.Add(unturnedItemId, shopItem);

            OnShopItemAdded?.Invoke(this, new ShopItemEventArgs(shopItem));
            return shopItem;
        }

        public bool RemoveItem(ushort unturnedItemId)
        {
            Dictionary<ushort, ShopItem> shopItems = dataManager.GameData.ShopItems;
            if (!shopItems.ContainsKey(unturnedItemId))
                return false;

            ShopItem shopItem = shopItems[unturnedItemId];

            OnShopItemRemoved?.Invoke(this, new ShopItemEventArgs(shopItem));
            return shopItems.Remove(unturnedItemId);
        }

        public ShopItem GetItem(ushort unturnedItemId)
        {
            Dictionary<ushort, ShopItem> shopItems = dataManager.GameData.ShopItems;
            if (!shopItems.ContainsKey(unturnedItemId))
                return null;

            return shopItems[unturnedItemId];
        }

        public ShopItem GetItemByName(string name, bool exactMatch = true)
        {
            Dictionary<ushort, ShopItem> shopItems = dataManager.GameData.ShopItems;
            if (exactMatch)
                return shopItems.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return shopItems.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public ShopItem[] GetItemList()
        {
            return dataManager.GameData.ShopItems.Values.ToArray();
        }

        public int GetItemCount()
        {
            return dataManager.GameData.ShopItems.Count();
        }

        public ShopItem ResolveItem(string shopItemNameOrId, bool exactMatch)
        {
            ushort unturnedItemId;
            if (ushort.TryParse(shopItemNameOrId, out unturnedItemId))
            {
                ShopItem shopItem = GetItem(unturnedItemId);
                if (shopItem != null)
                    return shopItem;
            }
            return GetItemByName(shopItemNameOrId, exactMatch);
        }

        public void SetItemPrice(ShopItem shopItem, double price)
        {
            shopItem.SetPrice(price);
            OnShopItemPriceChanged?.Invoke(this, new ShopItemEventArgs(shopItem));
        }

        public string GetItemSummary(ShopItem shopItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Nazwa: \"{shopItem.Name}\" | ID: {shopItem.UnturnedItemId}");
            sb.AppendLine($"Opis: \"{shopItem.Description}\"");
            sb.AppendLine($"Cena: ${shopItem.Price}");

            return sb.ToString();
        }

        public bool CanBuyItem(ShopItem shopItem, PlayerData buyer, byte amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (!buyer.TeamId.HasValue)
                return false;

            Team team = teamManager.GetTeam(buyer.TeamId.Value);
            double finalPrice = shopItem.Price * amount;
            return team.BankBalance >= finalPrice;
        }

        public bool BuyItem(ShopItem shopItem, PlayerData buyer, byte amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (!buyer.TeamId.HasValue)
                return false;

            if (!CanBuyItem(shopItem, buyer, amount))
                return false;

            Team team = teamManager.GetTeam(buyer.TeamId.Value);
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID((CSteamID)buyer.Id);

            if (!player.GiveItem(shopItem.UnturnedItemId, amount))
                throw new Exception($"Nie udało się dodać {shopItem.Name} (x{amount}) do ekwipunku gracza, konto nieobciążone");

            teamManager.WithdrawFromBank(team, shopItem.Price * amount);

            OnShopItemBought?.Invoke(this, new BuyItemEventArgs(shopItem, buyer, team, amount, shopItem.Price * amount));
            return true;
        }
    }
}
