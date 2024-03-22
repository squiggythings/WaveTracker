using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class Checkbox : Clickable {
        public bool Value { get; set; }

        public bool ValueWasChangedInternally;

        public Checkbox(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            width = 9;
            height = 9;
            SetParent(parent);
        }

        public void Update() {
            if (Clicked) {
                Value = !Value;
                ValueWasChangedInternally = true;
            }
            else {
                ValueWasChangedInternally = false;
            }
        }

        Rectangle GetBounds(int num) {
            return new Rectangle(440, num * 9, 9, 9);
        }

        public void DrawAsTabToggle() {
            if (Value) {
                DrawSprite(0, 0, GetBounds(3));
            }
            else {
                DrawSprite(0, 0, GetBounds(6));
            }
        }
        public void Draw() {
            if (enabled) {
                if (Value) {
                    if (IsHovered) {
                        if (IsPressed)
                            DrawSprite(0, 0, GetBounds(5));
                        else
                            DrawSprite(0, 0, GetBounds(4));
                    }
                    else {
                        DrawSprite(0, 0, GetBounds(3));
                    }
                }
                else {
                    if (IsHovered) {
                        if (IsPressed)
                            DrawSprite(0, 0, GetBounds(2));
                        else
                            DrawSprite(0, 0, GetBounds(1));
                    }
                    else {
                        DrawSprite(0, 0, GetBounds(0));
                    }
                }

            }
            else {
                if (Value)
                    DrawSprite(0, 0, GetBounds(7));
                else
                    DrawSprite(0, 0, GetBounds(6));
            }

        }
    }
}
