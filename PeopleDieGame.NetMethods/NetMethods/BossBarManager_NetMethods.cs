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
    [NetInvokableGeneratedClass(typeof(BossBarManager))]
    public static class BossBarManager_NetMethods
    {
        [NetInvokableGeneratedMethod("ReceiveUpdateBossBar", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveUpdateBossBar_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadString(out string name))
                return;
            if (!reader.ReadFloat(out float health))
                return;

            BossBarManager.ReceiveUpdateBossBar(name, health);
        }

        [NetInvokableGeneratedMethod("ReceiveUpdateBossBar", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveUpdateBossBar_Write(NetPakWriter writer, string name, float health)
        {
            writer.WriteString(name);
            writer.WriteFloat(health);
        }

        [NetInvokableGeneratedMethod("ReceiveRemoveBossBar", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveRemoveBossBar_Read(in ClientInvocationContext context)
        {
            BossBarManager.ReceiveRemoveBossBar();
        }

        [NetInvokableGeneratedMethod("ReceiveRemoveBossBar", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveRemoveBossBar_Write(NetPakWriter writer)
        {
        }
    }
}
