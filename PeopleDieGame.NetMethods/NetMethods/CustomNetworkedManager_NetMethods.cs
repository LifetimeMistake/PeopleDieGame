using PeopleDieGame.NetMethods.Managers;
using SDG.NetPak;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.NetMethods
{
    [NetInvokableGeneratedClass(typeof(CustomNetworkedManager))]
    public static class CustomNetworkedManager_NetMethods
    {
        [NetInvokableGeneratedMethod("ReceiveCoolFeatureRequest", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveCoolFeatureRequest_Read(in ClientInvocationContext context)
        {
            string amongus;
            context.reader.ReadString(out amongus);
            CustomNetworkedManager.ReceiveCoolFeatureRequest(amongus);
        }

        [NetInvokableGeneratedMethod("ReceiveCoolFeatureRequest", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveCoolFeatureRequest_Write(NetPakWriter writer, string amongus)
        {
            writer.WriteString(amongus);
        }
    }
}
