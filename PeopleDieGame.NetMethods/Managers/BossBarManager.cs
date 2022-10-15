using PeopleDieGame.NetMethods.Models;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Managers
{
    public class BossBarManager
    {
        private static readonly ClientStaticMethod<string, float> SendUpdateBossBar = ClientStaticMethod<string, float>.Get(new ClientStaticMethod<string, float>.ReceiveDelegate(ReceiveUpdateBossBar));
        private static readonly ClientStaticMethod SendRemoveBossBar = ClientStaticMethod.Get(new ClientStaticMethod.ReceiveDelegate(ReceiveRemoveBossBar));

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveUpdateBossBar(string name, float health)
        {
            Debug.Log($"Received BossBar {name} : {health}");
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveRemoveBossBar()
        {
            Debug.Log($"Received BossBar remove");
        }


        public static void UpdateBossBar(string name, float health, SteamPlayer player)
        {
            SendUpdateBossBar.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection, name, health);
        }

        public static void RemoveBossBar(SteamPlayer player)
        {
            SendRemoveBossBar.Invoke(SDG.NetTransport.ENetReliability.Reliable, player.transportConnection);
        }
    }
}
