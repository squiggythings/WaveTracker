﻿using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class ColorButton : Clickable {
        public Color Color { get; set; }
        public bool NeverShowAlpha { get; set; }
        public bool DrawBorder { get; set; }
        public string HexValue {
            get { return Color.GetHexCode(); }
            set { Color = Helpers.HexCodeToColor(value); }
        }

        public ColorButton(Color color, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            Color = color;
            width = 62;
            height = 13;
            DrawBorder = true;
            NeverShowAlpha = false;
            SetParent(parent);
        }

        public void Update() {
            if (Clicked) {
                Dialogs.colorPicker.Open(this);
            }
        }

        public void Draw() {
            Color displayColor = Color;
            if (IsPressed) {
                displayColor = displayColor.ToNegative();
            }

            Color textColor = (displayColor.R * 30 + displayColor.G * 59 + displayColor.B * 11) / 100 < 128 ? Color.White : Color.Black;
            if (DrawBorder) {
                DrawRect(0, 0, width, height, ButtonColors.backgroundColor);
            }
            DrawSprite(1, 1, width - 2, height - 2, new Rectangle(416, 208, 4, 2), Color.White);

            // draw color
            DrawRect(1, 1, width - 2, height - 2, displayColor);

            Color outlineColor = IsHovered ? textColor : Helpers.Alpha(Color.White, 80);
            DrawRect(1, 1, width - 2, 1, outlineColor);
            DrawRect(1, 2, 1, height - 4, outlineColor);
            DrawRect(1, height - 2, width - 2, 1, outlineColor);
            DrawRect(width - 2, 2, 1, height - 4, outlineColor);

            string label = "#" + (NeverShowAlpha ? Color.GetHexCodeIgnoringAlpha() : Color.GetHexCode());
            int labelWidth = Helpers.GetWidthOfText(label);
            Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4, textColor);
        }
    }

}
