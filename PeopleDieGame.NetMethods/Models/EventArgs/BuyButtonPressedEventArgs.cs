using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Models.EventArgs
{
    public class BuyButtonPressedEventArgs : System.EventArgs
    {
        public ushort ItemId;
        public byte Amount;

        public BuyButtonPressedEventArgs(ushort itemId, byte amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
