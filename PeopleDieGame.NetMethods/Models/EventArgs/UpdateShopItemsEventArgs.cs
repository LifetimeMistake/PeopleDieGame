using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Models.EventArgs
{
    public class UpdateShopItemsEventArgs : System.EventArgs
    {
        public Dictionary<ushort, float> Items;

        public UpdateShopItemsEventArgs(Dictionary<ushort, float> items)
        {
            Items = items;
        }
    }
}
