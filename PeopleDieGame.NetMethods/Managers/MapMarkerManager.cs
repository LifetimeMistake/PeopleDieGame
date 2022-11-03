using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.Reflection;
using SDG.Framework.UI.Sleek2;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Managers
{
    public static class MapMarkerManager
    {
        private static readonly ClientStaticMethod<MapMarker> sendMarker = ClientStaticMethod<MapMarker>.Get(new ClientStaticMethod<MapMarker>.ReceiveDelegate(ReceiveMarker));
        private static readonly ClientStaticMethod<Guid, Vector3> sendMarkerPosition = ClientStaticMethod<Guid, Vector3>.Get(new ClientStaticMethod<Guid, Vector3>.ReceiveDelegate(ReceiveMarkerPosition));
        private static readonly ClientStaticMethod<Guid, string> sendMarkerLabel = ClientStaticMethod<Guid, string>.Get(new ClientStaticMethod<Guid, string>.ReceiveDelegate(ReceiveMarkerLabel));
        private static readonly ClientStaticMethod<Guid, Color> sendMarkerColor = ClientStaticMethod<Guid, Color>.Get(new ClientStaticMethod<Guid, Color>.ReceiveDelegate(ReceiveMarkerColor));
        private static readonly ClientStaticMethod<Guid> sendRemoveMarker = ClientStaticMethod<Guid>.Get(new ClientStaticMethod<Guid>.ReceiveDelegate(ReceiveRemoveMarker));

        private static Dictionary<Guid, MapMarker> markers = new Dictionary<Guid, MapMarker>();
        private static Dictionary<Guid, ISleekImage> markerImages = new Dictionary<Guid, ISleekImage>();
        private static MethodRef projectWorldPositionToMap = MethodRef.GetMethodRef(typeof(PlayerDashboardInformationUI), "ProjectWorldPositionToMap");
        private static FieldRef<ISleekElement> mapMarkersContainer = FieldRef.GetFieldRef<ISleekElement>(typeof(PlayerDashboardInformationUI), "mapMarkersContainer");

        private static void CreateMarkerObject(MapMarker marker)
        {
            Debug.Log($"Setting marker: {marker.Id}, \"{marker.Label}\", {marker.Position}, {marker.Color}");
            ISleekImage sleekImage = Glazier.Get().CreateImage(PlayerDashboardInformationUI.icons.load<Texture2D>("Marker"));
            sleekImage.positionOffset_X = -10;
            sleekImage.positionOffset_Y = -10;
            sleekImage.sizeOffset_X = 20;
            sleekImage.sizeOffset_Y = 20;

            string label = (marker.Label == null) ? "" : marker.Label;
            sleekImage.addLabel(label, ESleekSide.RIGHT);

            Vector2 vector = (Vector2)projectWorldPositionToMap.Invoke(marker.Position);
            sleekImage.positionScale_X = vector.x;
            sleekImage.positionScale_Y = vector.y;
            sleekImage.color = marker.Color;
            sleekImage.isVisible = true;

            markerImages.Add(marker.Id, sleekImage);
            mapMarkersContainer.Value.AddChild(sleekImage);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveMarker(MapMarker mapMarker)
        {
            if (markers.ContainsKey(mapMarker.Id))
                return;

            markers.Add(mapMarker.Id, mapMarker);
            CreateMarkerObject(mapMarker);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveMarkerPosition(Guid markerId, Vector3 newPosition)
        {
            if (!markers.ContainsKey(markerId))
                return;

            markers[markerId].Position = newPosition;
            ISleekImage sleekImage = markerImages[markerId];
            Vector2 vector = (Vector2)projectWorldPositionToMap.Invoke(newPosition);
            sleekImage.positionScale_X = vector.x;
            sleekImage.positionScale_Y = vector.y;
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveMarkerLabel(Guid markerId, string label)
        {
            if (!markers.ContainsKey(markerId))
                return;

            label = (label == null) ? "" : label;
            markers[markerId].Label = label;
            ISleekImage sleekImage = markerImages[markerId];
            sleekImage.updateLabel(label);
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveMarkerColor(Guid markerId, Color color)
        {
            if (!markers.ContainsKey(markerId))
                return;

            markers[markerId].Color = color;
            ISleekImage sleekImage = markerImages[markerId];
            sleekImage.color = color;
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveRemoveMarker(Guid markerId)
        {
            if (!markers.ContainsKey(markerId))
                return;

            ISleekElement marker = markerImages[markerId];
            mapMarkersContainer.Value.RemoveChild(marker);
            markers.Remove(markerId);
            markerImages.Remove(markerId);
        }

        public static void SyncMarkersToPlayer(SteamPlayer player)
        {
            if (!Provider.isServer)
                return;

            foreach(MapMarker mapMarker in markers.Values)
                sendMarker.Invoke(ENetReliability.Reliable, player.transportConnection, mapMarker);
        }

        public static void ClearMarkers()
        {
            if (!Provider.isServer)
                return;

            foreach (MapMarker mapMarker in markers.Values)
                sendRemoveMarker.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), mapMarker.Id);
        }

        public static MapMarker CreateMarker(Vector3 position, string label = null, Color color = default)
        {
            if (!Provider.isServer)
                return null;

            MapMarker mapMarker = new MapMarker(Guid.NewGuid(), position, label, color);
            markers.Add(mapMarker.Id, mapMarker);
            sendMarker.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), mapMarker);
            return mapMarker;
        }

        public static void UpdateMarkerPosition(Guid markerId, Vector3 position)
        {
            if (!Provider.isServer)
                return;

            if (!markers.ContainsKey(markerId))
                return;

            markers[markerId].Position = position;
            sendMarkerPosition.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), markerId, position);
        }

        public static void UpdateMarkerLabel(Guid markerId, string label)
        {
            if (!Provider.isServer)
                return;

            if (!markers.ContainsKey(markerId))
                return;

            markers[markerId].Label = label;
            sendMarkerLabel.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), markerId, label);
        }

        public static void UpdateMarkerColor(Guid markerId, Color color)
        {
            if (!Provider.isServer)
                return;

            if (!markers.ContainsKey(markerId))
                return;

            markers[markerId].Color = color;
            sendMarkerColor.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), markerId, color);
        }

        public static void RemoveMarker(Guid markerId)
        {
            if (!Provider.isServer)
                return;

            if (!markers.ContainsKey(markerId))
                return;

            markers.Remove(markerId);
            sendRemoveMarker.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), markerId);
        }

        public static List<MapMarker> GetMarkers()
        {
            return markers.Values.ToList();
        }
    }
}
