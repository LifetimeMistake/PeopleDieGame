using PeopleDieGame.NetMethods.Managers;
using SDG.NetPak;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.NetMethods
{
    [NetInvokableGeneratedClass(typeof(InviteManager))]
    public static class InviteManager_NetMethods
    {
        [NetInvokableGeneratedMethod("ReceiveInviteRequest", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveInviteRequest_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadString(out string inviterName))
                return;

            if (!reader.ReadString(out string teamName))
                return;

            if (!reader.ReadFloat(out float inviteTTL))
                return;

            InviteManager.ReceiveInviteRequest(inviterName, teamName, inviteTTL);
        }

        [NetInvokableGeneratedMethod("ReceiveInviteRequest", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveInviteRequest_Write(NetPakWriter writer, string inviterName, string teamName, float inviteTTL)
        {
            writer.WriteString(inviterName);
            writer.WriteString(teamName);
            writer.WriteFloat(inviteTTL);
        }

        [NetInvokableGeneratedMethod("ReceiveInviteRequestResponse", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveInviteRequestResponse_Read(in ServerInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadBit(out bool result))
                return;

            InviteManager.ReceiveInviteRequestResponse(context, result);
        }

        [NetInvokableGeneratedMethod("ReceiveInviteRequestResponse", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveInviteRequestResponse_Write(NetPakWriter writer, bool result)
        {
            writer.WriteBit(result);
        }
    }
}
