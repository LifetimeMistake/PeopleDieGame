using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.Models.EventArgs;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.RPCs
{
    public static class ShopRPC
    {
        public static event EventHandler<ItemPurchaseRequestEventArgs> OnItemPurchaseRequested;
        public static event EventHandler<UpdateShopItemsEventArgs> OnUpdateShopItems;
        private static readonly ClientStaticMethod<Dictionary<ushort, float>> sendUpdateShopItems = ClientStaticMethod<Dictionary<ushort, float>>.Get(new ClientStaticMethod<Dictionary<ushort, float>>.ReceiveDelegate(ReceiveUpdateShopItems));
        private static readonly ServerStaticMethod<ushort, byte> sendItemPurchaseRequest = ServerStaticMethod<ushort, byte>.Get(new ServerStaticMethod<ushort, byte>.ReceiveDelegateWithContext(ReceiveItemPurchaseRequest));

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

        public static void RequestItemPurchase(ushort itemId, byte amount)
        {
            if (!Provider.isClient)
                return;

            sendItemPurchaseRequest.Invoke(SDG.NetTransport.ENetReliability.Reliable, itemId, amount);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveUpdateShopItems(Dictionary<ushort, float> items)
        {
            OnUpdateShopItems?.Invoke(null, new UpdateShopItemsEventArgs(items));
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public static void ReceiveItemPurchaseRequest(in ServerInvocationContext context, ushort itemId, byte amount)
        {
            OnItemPurchaseRequested?.Invoke(null, new ItemPurchaseRequestEventArgs(context.GetCallingPlayer(), itemId, amount));
        }
    }
}
