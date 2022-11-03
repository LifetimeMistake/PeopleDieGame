using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.NetMethods.Models.EventArgs;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Managers
{
    public static class InviteManager
    {
        private static readonly ClientStaticMethod<string, string, float> sendInviteRequest = ClientStaticMethod<string, string, float>.Get(new ClientStaticMethod<string, string, float>.ReceiveDelegate(ReceiveInviteRequest));
        private static readonly ServerStaticMethod<bool> sendInviteRequestResponse = ServerStaticMethod<bool>.Get(new ServerStaticMethod<bool>.ReceiveDelegateWithContext(ReceiveInviteRequestResponse));

        public static event EventHandler<InviteResponseEventArgs> OnInviteAccepted;
        public static event EventHandler<InviteResponseEventArgs> OnInviteRejected;

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveInviteRequest(string inviterName, string teamName, float inviteTTL)
        {
            InviteRequestUI.Open(inviterName, teamName, inviteTTL);
        }

        [SteamCall(ESteamCallValidation.SERVERSIDE)]
        public static void ReceiveInviteRequestResponse(in ServerInvocationContext context, bool result)
        {
            InviteResponseEventArgs eventArgs = new InviteResponseEventArgs(context.GetCallingPlayer());
            if (result)
                OnInviteAccepted?.Invoke(null, eventArgs);
            else
                OnInviteRejected?.Invoke(null, eventArgs);
        }

        public static void SendInviteRequest(SteamPlayer player, string inviterName, string teamName, float inviteTTL)
        {
            if (!Provider.isServer)
                return;

            sendInviteRequest.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection, inviterName, teamName, inviteTTL);
        }
        
        public static void SendInviteResponse(bool result)
        {
            if (!Provider.isClient)
                return;

            sendInviteRequestResponse.Invoke(SDG.NetTransport.ENetReliability.Reliable, result);
        }
    }
}
