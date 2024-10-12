using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class TextBlock : Clickable {
        public string Text { get; set; }
        public Color TextColour { get; set; }
        public bool DrawBorder { get; set; }

        public TextBlock(string text, int x, int y, int width, int height, Element parent, bool drawBorder = false) {
            enabled = true;
            this.x = x;
            this.y = y;
            Text = text;
            this.width = width;
            this.height = height;
            TextColour = UIColors.black;
            DrawBorder = drawBorder;
            SetTooltip(text, text);
            SetParent(parent);
        }

        public TextBlock(string text, int x, int y, int width, int height, Element parent, Color color, bool drawBorder = false) {
            enabled = true;
            this.x = x;
            this.y = y;
            Text = text;
            this.width = width;
            this.height = height;
            TextColour = color;
            DrawBorder = drawBorder;
            SetTooltip(text, text);
            SetParent(parent);
        }

        public void Update() {
            if (IsMouseOverRegion) {
                if (TooltipTextLong != "") {
                    Tooltip.TooltipTextLong = Text;
                }

                if (TooltipText != "") {
                    Tooltip.TooltipText = Text;
                }
            }
        }

        public void Draw() {
            //Draw border
            if (DrawBorder) {
                DrawRect(0, 0, width, height, UIColors.label);
            }

            //Draw text
            StartRectangleMask(2, 0, width - 2, height);
            WriteMultiline(Text, 2, 0, width - 2, TextColour);
            EndRectangleMask();
        }
    }
}
