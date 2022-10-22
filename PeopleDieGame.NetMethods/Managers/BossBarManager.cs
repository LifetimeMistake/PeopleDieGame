using PeopleDieGame.NetMethods.Models;
using PeopleDieGame.Reflection;
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
        private static bool showBar = false;
        private static FieldRef<ISleekElement> container = FieldRef.GetFieldRef<ISleekElement>(typeof(PlayerLifeUI), "_container");
        private static ISleekElement frame;
        private static AdvancedSleekProgress healthBar;
        private static ISleekLabel label;

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveUpdateBossBar(string name, float health)
        {
            if (!showBar)
            {
                frame = Glazier.Get().CreateBox();
                frame.sizeScale_X = 0.3f;
                frame.sizeScale_Y = 0.08f;
                frame.positionScale_X = 0.5f - (frame.sizeScale_X / 2);
                frame.positionOffset_Y = 10;

                healthBar = new AdvancedSleekProgress(health);
                healthBar.positionOffset_X = 10;
                healthBar.positionOffset_Y = 10;
                healthBar.sizeScale_X = 1f;
                healthBar.sizeScale_Y = 0.5f;
                healthBar.sizeOffset_X = -healthBar.positionOffset_X * 2;
                healthBar.Color = Palette.COLOR_R;
                healthBar.DifferenceColor = new Color(255 / 255f, 201 / 255f, 14 / 255f);

                label = Glazier.Get().CreateLabel();
                label.fontAlignment = TextAnchor.MiddleCenter;
                label.text = "";
                label.positionOffset_X = 10;
                label.positionOffset_Y = 5;
                label.positionScale_Y = 0.5f;
                label.sizeScale_X = 1f;
                label.sizeScale_Y = 0.5f;
                label.sizeOffset_X = -label.positionOffset_X * 2;
                label.fontSize = ESleekFontSize.Large;
                label.shadowStyle = ETextContrastContext.ColorfulBackdrop;

                frame.AddChild(healthBar);
                frame.AddChild(label);
                container.Value.AddChild(frame);
                showBar = true;
            }

            healthBar.Value = health;
            label.text = name;
        }

        [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
        public static void ReceiveRemoveBossBar()
        {
            if (!showBar)
                return;

            frame.RemoveAllChildren();
            container.Value.RemoveChild(frame);
            frame = null;
            healthBar = null;
            label = null;
            showBar = false;
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
