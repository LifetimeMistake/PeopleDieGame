using PeopleDieGame.NetMethods.RPCs;
using SDG.NetPak;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.NetMethods
{
    [NetInvokableGeneratedClass(typeof(ShopRPC))]
    public static class ShopRPC_NetMethods
    {
        [NetInvokableGeneratedMethod("ReceiveUpdateShopItems", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveUpdateShopItems_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadInt32(out int count))
                return;

            Dictionary<ushort, float> items = new Dictionary<ushort, float>();
            for (int i = 0; i < count; i++)
            {
                if (!reader.ReadUInt16(out ushort itemId))
                    return;

                if (!reader.ReadFloat(out float price))
                    return;

                items.Add(itemId, price);
            }

            ShopRPC.ReceiveUpdateShopItems(items);
        }

        [NetInvokableGeneratedMethod("ReceiveUpdateShopItems", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveUpdateShopItems_Write(NetPakWriter writer, Dictionary<ushort, float> items)
        {
            writer.WriteInt32(items.Count);
            foreach (KeyValuePair<ushort, float> kvp in items)
            {
                writer.WriteUInt16(kvp.Key);
                writer.WriteFloat(kvp.Value);
            }
        }

        [NetInvokableGeneratedMethod("ReceiveItemPurchaseRequest", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveRequestItemPurchase_Read(in ServerInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadUInt16(out ushort itemId))
                return;

            if (!reader.ReadUInt8(out byte amount))
                return;

            ShopRPC.ReceiveItemPurchaseRequest(context, itemId, amount);
        }

        [NetInvokableGeneratedMethod("ReceiveItemPurchaseRequest", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveRequestItemPurchase_Write(NetPakWriter writer, ushort itemId, byte amount)
        {
            writer.WriteUInt16(itemId);
            writer.WriteUInt8(amount);
        }
    }
}
