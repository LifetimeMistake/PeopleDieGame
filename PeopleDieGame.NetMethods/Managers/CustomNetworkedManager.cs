using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Managers
{
    public class CustomNetworkedManager
    {
        private static readonly ClientStaticMethod<string> SendCoolFeature = ClientStaticMethod<string>.Get(new ClientStaticMethod<string>.ReceiveDelegate(ReceiveCoolFeatureRequest));
        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveCoolFeatureRequest(string amongus)
        {
            Debug.Log($"got it: {amongus}");
        }

        public static void SendCoolFeatureRequest(string amongus)
        {
            SendCoolFeature.Invoke(SDG.NetTransport.ENetReliability.Reliable, Provider.EnumerateClients_Remote(), amongus);
        }
    }
}
