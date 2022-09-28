using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Enums;

namespace UnturnedGameMaster.Models
{
    public class CachedItem
    {
        public ushort Id { get; set; }
        public UnturnedPlayer Player { get; set; }
        public InteractableVehicle Vehicle { get; set; }
        public InteractableStorage Storage { get; set; }
        public RegionItem RegionItem { get; set; }
        public CachedItemState State { get; set; }

        public CachedItem(ushort id)
        {
            Id = id;
        }

        public CachedItemState GetLocation(bool validateCache = true)
        {
            if (!ValidateCache())
            {
                if (validateCache == true)
                    RebuildCache();
            }
            return State;
        }

        public bool ValidateCache()
        {
            bool valid;

            switch (State)
            {
                case CachedItemState.Ground:
                    if (RegionItem == null)
                    {
                        valid = false;
                        break;
                    }
                    valid = RegionItem.Region.items.Contains(RegionItem.ItemData);
                    break;
                case CachedItemState.Player:
                    if (Player == null)
                    {
                        valid = false;
                        break;
                    }
                    InventorySearch search = Player.Inventory.has(Id);
                    valid = search != null;
                    break;
                case CachedItemState.Vehicle:
                    if (Vehicle == null)
                    {
                        valid = false;
                        break;
                    }
                    search = Vehicle.trunkItems.has(Id);
                    valid = search != null;
                    break;
                case CachedItemState.Storage:
                    if (Storage == null)
                    {
                        valid = false;
                        break;
                    }
                    search = Storage.items.has(Id);
                    valid = search != null;
                    break;
                default:
                    valid = false;
                    break;
            }

            return valid;
        }

        public void RebuildCache()
        {
            SetRegionItem();
            SetPlayer();
            SetVehicle();
            SetStorage();
            
            if (Player == null && Vehicle == null && Storage == null && RegionItem == null)
            {
                State = CachedItemState.Unknown;
            }
        }

        private void SetRegionItem()
        {
            RegionItem = ItemLocator.GetDroppedItems(Id).FirstOrDefault();
            if (RegionItem != null)
            {
                State = CachedItemState.Ground;
                Player = null;
                Vehicle = null;
                Storage = null;
                return;
            }
        }

        private void SetPlayer()
        {
            Player = ItemLocator.GetPlayersWithItem(Id).FirstOrDefault();
            if (Player != null)
            {
                State = CachedItemState.Player;
                RegionItem = null;
                Vehicle = null;
                Storage = null;
                return;
            }
        }

        private void SetVehicle()
        {
            Vehicle = ItemLocator.GetVehiclesWithItem(Id).FirstOrDefault();
            if (Vehicle != null)
            {
                State = CachedItemState.Vehicle;
                Player = null;
                RegionItem = null;
                Storage = null;
                return;
            }
        }

        private void SetStorage()
        {
            Storage = ItemLocator.GetStoragesWithItem(Id).FirstOrDefault();
            if (Storage != null)
            {
                State = CachedItemState.Storage;
                Player = null;
                RegionItem = null;
                Vehicle = null;
                return;
            }
        }
    }
}
