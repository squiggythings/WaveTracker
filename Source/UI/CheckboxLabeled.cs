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
    public class CheckboxLabeled : Clickable {
        public bool Value { get; set; }
        string label;

        public CheckboxLabeled(string label, int x, int y, int width, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.label = label;
            height = 11;
            SetParent(parent);
        }

        public void Update() {
            if (Clicked) {
                Value = !Value;
            }
        }

        Rectangle GetBounds(int num) {
            return new Rectangle(440, num * 9, 9, 9);
        }

        public void Draw() {
            if (enabled) {
                if (Value) {
                    if (IsHovered) {
                        if (IsPressed)
                            DrawSprite(0, (height - 9) / 2, GetBounds(5));
                        else
                            DrawSprite(0, (height - 9) / 2, GetBounds(4));
                    }
                    else {
                        DrawSprite(0, (height - 9) / 2, GetBounds(3));
                    }
                }
                else {
                    if (IsHovered) {
                        if (IsPressed)
                            DrawSprite(0, (height - 9) / 2, GetBounds(2));
                        else
                            DrawSprite(0, (height - 9) / 2, GetBounds(1));
                    }
                    else {
                        DrawSprite(0, (height - 9) / 2, GetBounds(0));
                    }
                }

            }
            else {
                if (Value)
                    DrawSprite(0, (height - 9) / 2, GetBounds(7));
                else
                    DrawSprite(0, (height - 9) / 2, GetBounds(6));
            }
            Color labelCol = UIColors.labelDark;
            if (IsHovered)
                labelCol = Color.Black;
            Write(label + "", 13, (height - 7) / 2, labelCol);
        }

    }
}
