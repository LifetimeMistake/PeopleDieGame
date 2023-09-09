using PeopleDieGame.NetMethods.RPCs;
using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.ServerPlugin.Models;
using PeopleDieGame.ServerPlugin.Services.Managers;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public class ChaseManager : IDisposableService
    {
        [InjectDependency]
        private ObjectiveManager objectiveManager { get; set; }
        [InjectDependency]
        private TimerManager timerManager { get; set; }

        private Dictionary<ushort, MapMarker> markers = new Dictionary<ushort, MapMarker>();

        public void Init()
        {
            objectiveManager.ObjectiveItemSpawned += ObjectiveManager_ObjectiveItemSpawned;
            objectiveManager.ObjectiveItemRemoved += ObjectiveManager_ObjectiveItemRemoved;
            timerManager.Register(UpdateMarkers, 60);
        }

        public void Dispose()
        {
            objectiveManager.ObjectiveItemSpawned -= ObjectiveManager_ObjectiveItemSpawned;
            objectiveManager.ObjectiveItemRemoved -= ObjectiveManager_ObjectiveItemRemoved;
            timerManager.Unregister(UpdateMarkers);
        }
        
        private void CreateOrUpdateMarker(ushort itemId)
        {
            ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;
            CachedItem item = objectiveManager.GetItemCache(itemId);

            Vector3 offset;
            string stateLabel;
            switch (item.GetLocation())
            {
                case CachedItemLocation.Ground:
                    stateLabel = "porzucony na ziemi";
                    offset = new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10));
                    break;
                case CachedItemLocation.Player:
                    offset = new Vector3(UnityEngine.Random.Range(-100, 100), 0, UnityEngine.Random.Range(-100, 100));
                    stateLabel = $"w rękach {item.Player.CharacterName}";
                    break;
                case CachedItemLocation.Vehicle:
                    offset = new Vector3(UnityEngine.Random.Range(-50, 50), 0, UnityEngine.Random.Range(-50, 50));
                    stateLabel = $"ukryty w bagażniku";
                    break;
                default:
                    return;
            }

            Vector3? position = objectiveManager.GetObjectiveItemPosition(itemId);
            if (!position.HasValue)
                return; // ??????

            position = position + offset;
            string label = $"{itemAsset.FriendlyName}, {stateLabel}";

            if(!markers.ContainsKey(itemId))
            {
                MapMarker marker = MapMarkerManager.CreateMarker(position.Value, label, Color.red);
                markers.Add(itemId, marker);
            }
            else
            {
                MapMarker marker = markers[itemId];
                MapMarkerManager.UpdateMarkerLabel(marker.Id, label);
                MapMarkerManager.UpdateMarkerPosition(marker.Id, position.Value);
            }
        }

        private void RemoveMarker(ushort itemId)
        {
            if (!markers.ContainsKey(itemId))
                return;

            MapMarker marker = markers[itemId];
            MapMarkerManager.RemoveMarker(marker.Id);
            markers.Remove(itemId);
        }

        private void ObjectiveManager_ObjectiveItemSpawned(object sender, Models.EventArgs.ObjectiveItemEventArgs e)
        {
            if (e.ObjectiveItem.State != Enums.ObjectiveState.Roaming)
                return;

            ushort itemId = e.ObjectiveItem.ItemId;
            if (markers.ContainsKey(itemId))
                return; // what?

            CreateOrUpdateMarker(itemId);
        }

        private void ObjectiveManager_ObjectiveItemRemoved(object sender, Models.EventArgs.ObjectiveItemEventArgs e)
        {
            ushort itemId = e.ObjectiveItem.ItemId;

            if (markers.ContainsKey(itemId))
            {
                MapMarker marker = markers[itemId];
                MapMarkerManager.RemoveMarker(marker.Id);
                markers.Remove(e.ObjectiveItem.ItemId);
            }
        }

        private void UpdateMarkers()
        {
            foreach(KeyValuePair<ushort, MapMarker> kvp in markers.ToList())
            {
                ushort itemId = kvp.Key;
                ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;
                CachedItem cachedItem = objectiveManager.GetItemCache(itemId);
                CachedItemLocation? location = cachedItem?.GetLocation();

                if (cachedItem == null || location == CachedItemLocation.Unknown || location == CachedItemLocation.Storage)
                {
                    RemoveMarker(itemId);
                    continue;
                }

                CreateOrUpdateMarker(itemId);
            }
        }
    }
}
