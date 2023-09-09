using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.ClientModule.ShopMenu.EventArgs
{
    public class RequestItemPurchaseEventArgs : System.EventArgs
    {
        public ushort ItemId;
        public byte Amount;

        public RequestItemPurchaseEventArgs(ushort itemId, byte amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
