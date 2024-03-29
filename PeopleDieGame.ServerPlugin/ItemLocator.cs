﻿using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Models;

namespace PeopleDieGame.ServerPlugin
{
    public static class ItemLocator
    {
        public static List<UnturnedPlayer> GetPlayersWithItem(ushort itemId)
        {
            List<UnturnedPlayer> playerList = new List<UnturnedPlayer>();

            foreach (SteamPlayer steamPlayer in Provider.clients)
            {
                playerList.Add(UnturnedPlayer.FromCSteamID(steamPlayer.playerID.steamID));
            }

            foreach (UnturnedPlayer player in playerList.ToList())
            {
                List<InventorySearch> searchList = player.Inventory.search(itemId, false, true);
                if (searchList.Count == 0)
                    playerList.Remove(player);
            }

            if (playerList.Count == 0)
                return new List<UnturnedPlayer>();

            return playerList;
        }

        public static List<RegionItem> GetDroppedItems(ushort itemId)
        {
            List<RegionItem> items = new List<RegionItem>();

            foreach (ItemRegion region in ItemManager.regions)
            {
                if (region.items.Count == 0)
                    continue;

                foreach (ItemData itemData in region.items)
                {
                    if (itemData.item.id == itemId && itemData.isDropped)
                    {
                        RegionItem item = new RegionItem(region, itemData);
                        items.Add(item);
                    }
                }
            }

            if (items.Count == 0)
                return new List<RegionItem>();

            return items;
        }

        public static List<InteractableStorage> GetStoragesWithItem(ushort itemId)
        {
            List<InteractableStorage> storageList = UnityEngine.Object.FindObjectsOfType<InteractableStorage>().ToList();

            foreach (InteractableStorage storage in storageList.ToList())
            {
                List<InventorySearch> searchList = storage.items.search(new List<InventorySearch>(), itemId, false, true);

                if (searchList.Count == 0)
                    storageList.Remove(storage);
            }

            if (storageList.Count == 0)
                return new List<InteractableStorage>();

            return storageList;
        }

        public static List<InteractableVehicle> GetVehiclesWithItem(ushort itemId)
        {
            List<InteractableVehicle> vehicleList = VehicleManager.vehicles;

            foreach (InteractableVehicle vehicle in vehicleList.ToList())
            {
                if (vehicle.trunkItems == null)
                {
                    vehicleList.Remove(vehicle);
                    continue;
                }

                List<InventorySearch> searchList = vehicle.trunkItems.search(new List<InventorySearch>(), itemId, false, true);

                if (searchList.Count == 0)
                    vehicleList.Remove(vehicle);
            }

            if (vehicleList.Count == 0)
                return new List<InteractableVehicle>();

            return vehicleList;
        }
    }
}
