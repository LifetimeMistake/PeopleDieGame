using PeopleDieGame.NetMethods.Managers;
using PeopleDieGame.NetMethods.Models;
using SDG.NetPak;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PeopleDieGame.NetMethods.NetMethods
{
    [NetInvokableGeneratedClass(typeof(MapMarkerManager))]
    public static class MapMarkerManager_NetMethods
    {
        [NetInvokableGeneratedMethod("ReceiveMarker", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveMarker_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadGuid(out Guid guid))
                return;
            if (!reader.ReadNormalVector3(out Vector3 position))
                return;
            if (!reader.ReadString(out string label))
                return;
            if (!reader.ReadColor32RGBA(out Color color))
                return;

            MapMarker mapMarker = new MapMarker(guid, position, label, color);
            MapMarkerManager.ReceiveMarker(mapMarker);
        }

        [NetInvokableGeneratedMethod("ReceiveMarker", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveMarker_Write(NetPakWriter writer, MapMarker marker)
        {
            writer.WriteGuid(marker.Id);
            writer.WriteClampedVector3(marker.Position);
            writer.WriteString(marker.Label);
            writer.WriteColor32RGBA(marker.Color);
        }

        [NetInvokableGeneratedMethod("ReceiveMarkerPosition", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveMarkerPosition_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadGuid(out Guid guid))
                return;
            if (!reader.ReadNormalVector3(out Vector3 position))
                return;

            MapMarkerManager.ReceiveMarkerPosition(guid, position);
        }

        [NetInvokableGeneratedMethod("ReceiveMarkerPosition", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveMarkerPosition_Write(NetPakWriter writer, Guid markerId, Vector3 newPosition)
        {
            writer.WriteGuid(markerId);
            writer.WriteNormalVector3(newPosition);
        }

        [NetInvokableGeneratedMethod("ReceiveMarkerLabel", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveMarkerLabel_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadGuid(out Guid guid))
                return;
            if (!reader.ReadString(out string label))
                return;

            MapMarkerManager.ReceiveMarkerLabel(guid, label);
        }

        [NetInvokableGeneratedMethod("ReceiveMarkerLabel", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveMarkerLabel_Write(NetPakWriter writer, Guid markerId, string label)
        {
            writer.WriteGuid(markerId);
            writer.WriteString(label);
        }

        [NetInvokableGeneratedMethod("ReceiveMarkerColor", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveMarkerColor_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadGuid(out Guid guid))
                return;
            if (!reader.ReadColor32RGBA(out Color color))
                return;

            MapMarkerManager.ReceiveMarkerColor(guid, color);
        }

        [NetInvokableGeneratedMethod("ReceiveMarkerColor", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveMarkerColor_Write(NetPakWriter writer, Guid markerId, Color color)
        {
            writer.WriteGuid(markerId);
            writer.WriteColor32RGBA(color);
        }

        [NetInvokableGeneratedMethod("ReceiveRemoveMarker", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveRemoveMarker_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadGuid(out Guid guid))
                return;

            MapMarkerManager.ReceiveRemoveMarker(guid);
        }

        [NetInvokableGeneratedMethod("ReceiveRemoveMarker", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveRemoveMarker_Write(NetPakWriter writer, Guid markerId)
        {
            writer.WriteGuid(markerId);
        }
    }
}
