using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Models.EventArgs
{
    public class ItemPurchaseRequestEventArgs : System.EventArgs
    {
        public SteamPlayer Caller;
        public ushort ItemId;
        public byte Amount;

        public ItemPurchaseRequestEventArgs(SteamPlayer caller, ushort itemid, byte amount)
        {
            Caller = caller ?? throw new ArgumentNullException(nameof(caller));
            ItemId = itemid;
            Amount = amount;
        }
    }
}
