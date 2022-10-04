using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Enums;

namespace PeopleDieGame.ServerPlugin.Models
{
    public class CachedItem
    {
        public ushort Id { get; private set; }
        public UnturnedPlayer Player { get; private set; }
        public InteractableVehicle Vehicle { get; private set; }
        public InteractableStorage Storage { get; private set; }
        public RegionItem RegionItem { get; private set; }
        private CachedItemState Location { get; set; }

        public CachedItem(ushort id)
        {
            Id = id;
        }

        public CachedItemState GetLocation(bool validateCache = true)
        {
            if (!ValidateCache())
            {
                if (validateCache == true)
                {
                    RebuildCache();
                    return GetLocation(false);
                }
                return CachedItemState.Unknown;
            }
            return Location;
        }

        public bool ValidateCache()
        {
            switch (Location)
            {
                case CachedItemState.Ground:
                    return (RegionItem != null && RegionItem.Region.items.Contains(RegionItem.ItemData));
                case CachedItemState.Player:
                    return (Player != null && Player.Inventory.has(Id) != null);
                case CachedItemState.Vehicle:
                    return (Vehicle != null && Vehicle.trunkItems.has(Id) != null);
                case CachedItemState.Storage:
                    return (Storage != null && Storage.items.has(Id) != null);
            }

            return false;
        }

        public void RebuildCache()
        {
            List<RegionItem> regionItems = ItemLocator.GetDroppedItems(Id);
            RegionItem regionItem = regionItems.FirstOrDefault();
            if (regionItem != null)
            {
                SetDroppedItem(regionItem);
                return;
            }

            List<UnturnedPlayer> players = ItemLocator.GetPlayersWithItem(Id);
            UnturnedPlayer player = players.FirstOrDefault();
            if (player != null)
            {
                SetPlayer(player);
                return;
            }

            List<InteractableVehicle> vehicles = ItemLocator.GetVehiclesWithItem(Id);
            InteractableVehicle vehicle = vehicles.FirstOrDefault();
            if (vehicle != null)
            {
                SetVehicle(vehicle);
                return;
            }

            List<InteractableStorage> storages = ItemLocator.GetStoragesWithItem(Id);
            InteractableStorage storage = storages.FirstOrDefault();
            if (storage != null)
            {
                SetStorage(storage);
                return;
            }
            else
            {
                Location = CachedItemState.Unknown;
                RegionItem = null;
                Player = null;
                Vehicle = null;
                Storage = null;
            }
        }

        private void SetDroppedItem(RegionItem regionItem)
        {
            Location = CachedItemState.Ground;
            RegionItem = regionItem;
            Player = null;
            Vehicle = null;
            Storage = null;
        }

        private void SetPlayer(UnturnedPlayer player)
        {
            Location = CachedItemState.Player;
            RegionItem = null;
            Player = player;
            Vehicle = null;
            Storage = null;
        }

        private void SetVehicle(InteractableVehicle vehicle)
        {
            Location = CachedItemState.Vehicle;
            RegionItem = null;
            Player = null;
            Vehicle = vehicle;
            Storage = null;
        }

        private void SetStorage(InteractableStorage storage)
        {
            Location = CachedItemState.Storage;
            RegionItem = null;
            Player = null;
            Vehicle = null;
            Storage = storage;
        }
    }
}
