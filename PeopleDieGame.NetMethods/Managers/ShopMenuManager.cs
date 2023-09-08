using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.Models.EventArgs;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Managers
{
    public static class ShopMenuManager
    {
        public static event EventHandler<ItemPurchaseRequestEventArgs> OnItemPurchaseRequested;
        public static readonly ClientStaticMethod sendOpenShop = ClientStaticMethod.Get(new ClientStaticMethod.ReceiveDelegate(ReceiveOpenShop));
        public static readonly ClientStaticMethod<Dictionary<ushort, float>> sendUpdateShopItems = ClientStaticMethod<Dictionary<ushort, float>>.Get(new ClientStaticMethod<Dictionary<ushort, float>>.ReceiveDelegate(ReceiveUpdateShopItems));
        public static readonly ClientStaticMethod<float> sendBalanceUpdated = ClientStaticMethod<float>.Get(new ClientStaticMethod<float>.ReceiveDelegate(ReceiveUpdateBalance));
        public static readonly ServerStaticMethod<ushort, byte> sendItemPurchaseRequest = ServerStaticMethod<ushort, byte>.Get(new ServerStaticMethod<ushort, byte>.ReceiveDelegateWithContext(ReceiveItemPurchaseRequest));

        public static void OpenShopUI(SteamPlayer player)
        {
            if (!Provider.isServer)
                return;

            sendOpenShop.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection);
        }

        public static void UpdateShopItems(Dictionary<ushort, float> items)
        {
            if (!Provider.isServer)
                return;

            sendUpdateShopItems.Invoke(SDG.NetTransport.ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), items);
        }

        public static void UpdateShopItems(SteamPlayer player, Dictionary<ushort, float> items)
        {
            if (!Provider.isServer)
                return;

            sendUpdateShopItems.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection, items);
        }

        public static void UpdateBalance(SteamPlayer player, float teamBalance)
        {
            if (!Provider.isServer)
                return;

            sendBalanceUpdated.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection, teamBalance);
        }
        public static void RequestItemPurchase(ushort itemId, byte amount)
        {
            if (!Provider.isClient)
                return;

            sendItemPurchaseRequest.Invoke(SDG.NetTransport.ENetReliability.Reliable, itemId, amount);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveUpdateShopItems(Dictionary<ushort, float> items)
        {
            ShopUI.UpdateItems(items);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveOpenShop()
        {
            ShopUI.Open();
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveUpdateBalance(float teamBalance)
        {
            ShopUI.UpdateBalance(teamBalance);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public static void ReceiveItemPurchaseRequest(in ServerInvocationContext context, ushort itemId, byte amount)
        {
            OnItemPurchaseRequested?.Invoke(null, new ItemPurchaseRequestEventArgs(context.GetCallingPlayer(), itemId, amount));
        }
    }
}
