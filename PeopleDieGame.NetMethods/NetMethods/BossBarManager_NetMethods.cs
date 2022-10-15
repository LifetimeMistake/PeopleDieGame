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
        [NetInvokableGeneratedMethod("ReceiveBossBar", ENetInvokableGeneratedMethodPurpose.Read)]
        public static void ReceiveBossBar_Read(in ClientInvocationContext context)
        {
            NetPakReader reader = context.reader;
            if (!reader.ReadString(out string name))
                return;
            if (!reader.ReadFloat(out float health))
                return;
            if (!reader.ReadFloat(out float maxHealth))
                return;

            BossBarManager.ReceiveBossBar(name, health, maxHealth);
        }

        [NetInvokableGeneratedMethod("ReceiveBossBar", ENetInvokableGeneratedMethodPurpose.Write)]
        public static void ReceiveBossBar_Write(NetPakWriter writer, string name, float health, float maxHealth)
        {
            writer.WriteString(name);
            writer.WriteFloat(health);
            writer.WriteFloat(maxHealth);
        }
    }
}
