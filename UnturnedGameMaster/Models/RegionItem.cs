using SDG.Unturned;

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
