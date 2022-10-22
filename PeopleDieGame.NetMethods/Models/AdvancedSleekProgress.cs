using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods.Models
{
    public class AdvancedSleekProgress : SleekWrapper
    {
        private static StaticResourceRef<Texture2D> texture = new StaticResourceRef<Texture2D>("Materials/Pixel");
        private ISleekImage background;
        private ISleekImage difference;
        private ISleekImage foreground;
        private ISleekLabel label;
        private float differenceTarget;
        private float value;

        public Color Color
        {
            get
            {
                return foreground.color;
            }
            set
            {
                Color backgroundColor = value;
                backgroundColor.a *= 0.5f;
                background.color = backgroundColor;
                foreground.color = value;
            }
        }

        public Color DifferenceColor
        {
            get => difference.color;
            set => difference.color = value;
        }

        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = Mathf.Clamp01(value);
                UpdateValue();
            }
        }

        public bool LabelVisible
        {
            get => label.isVisible;
            set => label.isVisible = value;
        }

        public AdvancedSleekProgress(float value = 1f, bool labelVisible = true)
        {
            this.value = value;

            background = Glazier.Get().CreateImage();
            background.sizeScale_X = 1f;
            background.sizeScale_Y = 1f;
            background.texture = texture;
            AddChild(background);

            difference = Glazier.Get().CreateImage();
            difference.sizeScale_X = 1f;
            difference.sizeScale_Y = 1f;
            difference.texture = texture;
            AddChild(difference);

            foreground = Glazier.Get().CreateImage();
            foreground.sizeScale_X = 1f;
            foreground.sizeScale_Y = 1f;
            foreground.texture = texture;
            AddChild(foreground);

            label = Glazier.Get().CreateLabel();
            label.sizeScale_X = 1f;
            label.positionScale_Y = 0.5f;
            label.positionOffset_Y = -15;
            label.sizeOffset_Y = 30;
            label.textColor = Color.white;
            label.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            AddChild(label);

            label.isVisible = labelVisible;
            UpdateValue();
        }

        private void UpdateValue()
        {
            if (differenceTarget != value)
            {
                differenceTarget = value;
                if (differenceTarget < value)
                {
                    difference.sizeScale_X = value;
                }
                else
                {
                    difference.lerpSizeScale(value, difference.sizeScale_Y, ESleekLerp.LINEAR, 0.75f);
                }
            }

            foreground.sizeScale_X = value;
            label.text = Mathf.RoundToInt(foreground.sizeScale_X * 100f).ToString() + "%";
        }
    }
}
