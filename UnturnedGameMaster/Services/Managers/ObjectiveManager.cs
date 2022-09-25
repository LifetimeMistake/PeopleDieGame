using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Services.Managers
{
    internal class ObjectiveManager : IService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }

        public void Init()
        { }

        public bool AddObjectiveItem(ushort itemId)
        {
            ItemAsset item = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;

            if (item == null)
            {
                return false;
            }

            dataManager.GameData.ObjectiveItems.Add(itemId);
            return true;
        }

        public bool RemoveObjectiveItem(ushort itemId)
        {
            if (!GetObjectiveItems().Contains(itemId))
                return false;

            dataManager.GameData.ObjectiveItems.Remove(itemId);
            return true;
        }

        public bool SubmitObjectiveItems(PlayerData playerData)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID((CSteamID)playerData.Id);

            foreach (ushort itemId in GetObjectiveItems())
            {
                if (ItemLocator.GetPlayersWithItem(itemId).Contains(player))
                    continue;
                else
                    return false;
            }
            return true;
        }

        public ushort[] GetObjectiveItems()
        {
            return dataManager.GameData.ObjectiveItems.ToArray();
        }

        public List<Vector3S> GetObjectiveItemPosition(ushort itemId)
        {
            List<Vector3S> positions = new List<Vector3S>();

            if (!GetObjectiveItems().Contains(itemId))
                return positions;

            List<UnturnedPlayer> players = ItemLocator.GetPlayersWithItem(itemId);
            List<ItemData> items = ItemLocator.GetDroppedItems(itemId);
            List<InteractableStorage> storages = ItemLocator.GetStoragesWithItem(itemId);
            List<InteractableVehicle> vehicles = ItemLocator.GetVehiclesWithItem(itemId);

            if (items == null && players == null && storages == null && vehicles == null)
            {
                return null;
            }

            if (players != null)
            {
                foreach (UnturnedPlayer player in players)
                    positions.Add(player.Position);
            }
            if (items != null)
            {
                foreach (ItemData item in items)
                    positions.Add(item.point);
            }
            if (storages != null)
            {
                foreach (InteractableStorage storage in storages)
                    positions.Add(storage.gameObject.transform.position);
            }
            if (vehicles != null)
            {
                foreach (InteractableVehicle vehicle in vehicles)
                    positions.Add(vehicle.gameObject.transform.position);
            }

            return positions;
        }
    }
}
