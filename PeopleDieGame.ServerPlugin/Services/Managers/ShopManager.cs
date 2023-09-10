using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Models.EventArgs;
using PeopleDieGame.NetMethods.RPCs;
using UnityEngine;
using PeopleDieGame.NetMethods.Models.EventArgs;
using PeopleDieGame.ServerPlugin.Helpers;
using Rocket.Core.Steam;
using PeopleDieGame.NetMethods.Models;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    internal class ShopManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private GameManager gameManager { get; set; }

        public event EventHandler<ShopItemEventArgs> OnShopItemAdded;
        public event EventHandler<ShopItemEventArgs> OnShopItemRemoved;
        public event EventHandler<BuyItemEventArgs> OnShopItemBought;
        public event EventHandler<ShopItemEventArgs> OnShopItemPriceChanged;

        public void Init()
        {
            ShopRPC.OnItemPurchaseRequested += ShopMenuManager_OnItemPurchaseRequested;
        }

        public void Dispose()
        {
            ShopRPC.OnItemPurchaseRequested -= ShopMenuManager_OnItemPurchaseRequested;
        }

        private void ShopMenuManager_OnItemPurchaseRequested(object sender, ItemPurchaseRequestEventArgs e)
        {
            if (gameManager.GetGameState() != Enums.GameState.InGame)
                return;


            PlayerData playerData = playerDataManager.GetData((ulong)e.Caller.playerID.steamID);
            if (playerData == null)
            {
                Debug.LogWarning($"Failed to get player {e.Caller.playerID.steamID}'s data.");
                return;
            }

            if (!playerData.TeamId.HasValue)
            {
                Debug.LogWarning($"Player {playerData.Id} attempted to buy an item while not belonging to any team.");
                return;
            }

            ShopItem shopItem = GetItem(e.ItemId);

            if (shopItem == null)
            {
                Debug.LogWarning($"Player {playerData.Id} attempted to buy non-existent item {e.ItemId}");
                return;
            }

            if (!CanBuyItem(shopItem, playerData, e.Amount))
            {

                Debug.LogWarning($"Player {playerData.Id} attempted to buy item {e.ItemId} (x{e.Amount}) while having insufficient funds.");
                return;
            }

            if (!BuyItem(shopItem, playerData, e.Amount))
                return;

            ChatHelper.Say(UnturnedPlayer.FromSteamPlayer(e.Caller), $"Zakupiono {shopItem.Name} (x{e.Amount})");
        }

        private void BroadcastItemsUpdated()
        {
            Dictionary<ushort, float> items = GetSerializableItemList();
            ShopRPC.UpdateShopItems(items);
        }

        public ShopItem AddItem(ushort unturnedItemId, float price)
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
            BroadcastItemsUpdated();
            return shopItem;
        }

        public bool RemoveItem(ushort unturnedItemId)
        {
            Dictionary<ushort, ShopItem> shopItems = dataManager.GameData.ShopItems;
            if (!shopItems.ContainsKey(unturnedItemId))
                return false;

            ShopItem shopItem = shopItems[unturnedItemId];

            OnShopItemRemoved?.Invoke(this, new ShopItemEventArgs(shopItem));
            BroadcastItemsUpdated();
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

        public void SetItemPrice(ShopItem shopItem, float price)
        {
            shopItem.SetPrice(price);
            OnShopItemPriceChanged?.Invoke(this, new ShopItemEventArgs(shopItem));
            BroadcastItemsUpdated();
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

            teamManager.RemoveBalance(team, shopItem.Price * amount);

            OnShopItemBought?.Invoke(this, new BuyItemEventArgs(shopItem, buyer, team, amount, shopItem.Price * amount));
            return true;
        }

        public Dictionary<ushort, float> GetSerializableItemList()
        {
            Dictionary<ushort, ShopItem> shopItems = dataManager.GameData.ShopItems;
            Dictionary<ushort, float> serializableList = new Dictionary<ushort, float>();

            foreach (ShopItem item in shopItems.Values)
                serializableList.Add(item.UnturnedItemId, (float)item.Price);

            return serializableList;
        }
    }
}
