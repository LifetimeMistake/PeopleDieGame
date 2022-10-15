using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleDieGame.NetMethods.Managers
{
    public class BossBarManager
    {
        private static readonly ClientStaticMethod<string, float, float> SendBossBar = ClientStaticMethod<string, float, float>.Get(new ClientStaticMethod<string, float, float>.ReceiveDelegate(ReceiveBossBar));
        
        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveBossBar(string name, float health, float maxHealth)
        {
            
        }

        public static void CreateBossBar(string name, float health, float maxHealth, SteamPlayer player)
        {
            SendBossBar.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection, name, health, maxHealth);
        }
    }
}
