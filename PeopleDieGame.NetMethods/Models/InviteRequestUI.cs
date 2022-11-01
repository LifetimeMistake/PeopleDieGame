using PeopleDieGame.NetMethods.Managers;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models
{
    public static class InviteRequestUI
    {
        private static int PADDING = 10;
        private static float FADEIN_TIME = 0.3f;
        private static float FADEOUT_TIME = 0.15f;
        private static bool active;
        private static float timeout;
        private static float popupTime;

        private static ISleekElement container;
        private static ISleekImage timeoutBar;
        private static ISleekLabel titleLabel;
        private static ISleekLabel descriptionLabel;
        private static ISleekButton acceptButton;
        private static ISleekButton rejectButton;
        private static GameObject timerObject;
        private static UpdateBehaviour timer;

        static InviteRequestUI()
        {
            container = Glazier.Get().CreateBox();
            container.sizeScale_X = 0.4f;
            container.sizeScale_Y = 0.2f;
            container.positionScale_X = 0.5f - (container.sizeScale_X / 2);

            timeoutBar = Glazier.Get().CreateImage(Texture2D.whiteTexture);
            timeoutBar.sizeScale_X = 1f;
            timeoutBar.sizeScale_Y = 0.1f;
            container.AddChild(timeoutBar);

            titleLabel = Glazier.Get().CreateLabel();
            titleLabel.positionScale_Y = timeoutBar.sizeScale_Y;
            titleLabel.sizeScale_X = 1f;
            titleLabel.sizeOffset_Y = 80;
            titleLabel.positionOffset_Y = -PADDING;
            titleLabel.text = "Zaproszenie do drużyny";
            titleLabel.textColor = Color.white;
            titleLabel.fontSize = ESleekFontSize.Title;
            container.AddChild(titleLabel);

            descriptionLabel = Glazier.Get().CreateLabel();
            descriptionLabel.positionScale_Y = titleLabel.positionScale_Y;
            descriptionLabel.sizeScale_X = 1f;
            descriptionLabel.sizeOffset_Y = 80;
            descriptionLabel.positionOffset_Y = titleLabel.sizeOffset_Y + titleLabel.positionOffset_Y - PADDING;
            descriptionLabel.fontSize = ESleekFontSize.Large;
            descriptionLabel.textColor = Color.white;
            descriptionLabel.enableRichText = true;
            container.AddChild(descriptionLabel);

            acceptButton = Glazier.Get().CreateButton();
            acceptButton.sizeScale_X = 0.2f;
            acceptButton.sizeScale_Y = 0.2f;
            acceptButton.positionScale_X = 0.5f - acceptButton.sizeScale_X;
            acceptButton.positionScale_Y = 0.8f;
            acceptButton.positionOffset_X = -(PADDING / 2);
            acceptButton.positionOffset_Y = -PADDING;
            acceptButton.text = "(T) Dołącz";
            acceptButton.fontSize = ESleekFontSize.Large;
            acceptButton.fontAlignment = TextAnchor.MiddleCenter;
            acceptButton.textColor = Color.white;
            acceptButton.onClickedButton += AcceptButton_onClickedButton;
            container.AddChild(acceptButton);

            rejectButton = Glazier.Get().CreateButton();
            rejectButton.sizeScale_X = 0.2f;
            rejectButton.sizeScale_Y = 0.2f;
            rejectButton.positionScale_X = 0.5f;
            rejectButton.positionScale_Y = 0.8f;
            rejectButton.positionOffset_X = (PADDING / 2);
            rejectButton.positionOffset_Y = -PADDING;
            rejectButton.text = "(N) Odrzuć";
            rejectButton.fontSize = ESleekFontSize.Large;
            rejectButton.fontAlignment = TextAnchor.MiddleCenter;
            rejectButton.textColor = Color.white;
            rejectButton.onClickedButton += RejectButton_onClickedButton;
            container.AddChild(rejectButton);

            timerObject = new GameObject();
            timer = timerObject.AddComponent<UpdateBehaviour>();
            timer.OnFixedUpdate += Timer_OnFixedUpdate;

            PlayerUI.container.AddChild(container);
            Close();
        }

        private static void AcceptButton_onClickedButton(ISleekElement button)
        {
            if (!active)
                return;

            Accept();
        }

        private static void RejectButton_onClickedButton(ISleekElement button)
        {
            if (!active)
                return;

            Reject();
        }

        private static void Accept()
        {
            acceptButton.isClickable = true;
            rejectButton.isClickable = false;
            InviteManager.SendInviteResponse(true);
            Close();
        }

        private static void Reject()
        {
            acceptButton.isClickable = false;
            rejectButton.isClickable = true;
            InviteManager.SendInviteResponse(false);
            Close();
        }

        public static void Open(string inviterName, string teamName, float inviteTTL)
        {
            if (active)
                return;

            descriptionLabel.text = $"Przychodzące zaproszenie od <style=H3>{inviterName}</style>\ndo drużyny <style=H3>{teamName}</style>";

            timeoutBar.sizeScale_X = 1f;
            timeoutBar.lerpSizeScale(0, timeoutBar.sizeScale_Y, ESleekLerp.LINEAR, inviteTTL);
            container.positionScale_Y = -container.sizeScale_Y;
            container.lerpPositionScale(container.positionScale_X, -0.0075f, ESleekLerp.LINEAR, FADEIN_TIME);

            acceptButton.isClickable = true;
            rejectButton.isClickable = true;

            active = true;
            timeout = inviteTTL;
            popupTime = Time.realtimeSinceStartup;
            timer.Start();
        }

        public static void Close()
        {
            if (!active)
                return;

            container.positionScale_Y = -0.0075f;
            container.lerpPositionScale(container.positionScale_X, -container.sizeScale_Y, ESleekLerp.LINEAR, FADEOUT_TIME);

            active = false;
            timeout = 0;
            popupTime = 0;
            timer.Stop();
        }

        private static void Timer_OnFixedUpdate(object sender, System.EventArgs e)
        {
            if (!active)
                return;

            if (Time.realtimeSinceStartup >= popupTime + timeout)
            {
                Close();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Accept();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                Reject();
            }
        }
    }
}
