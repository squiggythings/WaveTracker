using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Rendering;

namespace WaveTracker.UI {
    public class ColorButton : Clickable {
        public static ColorPickerDialog colorPicker;
        public Color color { get; set; }
        public string HexValue {
            get { return color.GetHexCodeWithAlpha(); }
            set { color = Helpers.HexCodeToColor(value); }
        }

        public ColorButton(Color color, int x, int y, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.color = color;
            width = 45 + 10;
            //if (width < 30)
            //    width = 30;
            height = 13;
            SetParent(parent);
        }

        public void Update() {
            if (Clicked) {
                colorPicker.Open(this);
                //Random r = new Random();
                //color = new Color((byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256));

            }
        }



        public void Draw() {


            Color displayColor = color;
            if (IsPressed)
                displayColor = displayColor.ToNegative();
            Color textColor = Color.White;
            if ((displayColor.R * 30 + displayColor.G * 59 + displayColor.B * 11) / 100 < 128) {
                textColor = Color.White;
            } else {
                textColor = Color.Black;
            }
            DrawRect(0, 0, width, height, ButtonColors.Round.backgroundColor);

            // draw color
            DrawRect(1, 1, width - 2, height - 2, displayColor);

            Color outlineColor;
            if (IsHovered) {
                outlineColor = textColor;
            } else {
                outlineColor = Helpers.Alpha(Color.White, 80);
            }
            DrawRect(1, 1, width - 2, 1, outlineColor);
            DrawRect(1, 2, 1, height - 4, outlineColor);
            DrawRect(1, height - 2, width - 2, 1, outlineColor);
            DrawRect(width - 2, 2, 1, height - 4, outlineColor);

            string label = "#" + color.GetHexCode();
            int labelWidth = Helpers.getWidthOfText(label);
            Write(label, (width - labelWidth) / 2, (height + 1) / 2 - 4, textColor);
        }
    }

}
