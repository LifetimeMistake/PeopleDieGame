using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Models
{
    public class RegionItem
    {
        public ItemRegion Region { get; set; }
        public ItemData ItemData { get; set; }
        public RegionItem(ItemRegion region, ItemData itemData)
        {
            Region = region;
            ItemData = itemData;
        }
    }
}
