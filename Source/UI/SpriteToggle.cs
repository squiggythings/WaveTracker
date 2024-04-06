using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace WaveTracker.UI {
    public class SpriteToggle : Clickable {
        readonly int sourceX;
        readonly int sourceY;

        public bool Value { get; set; }

        public SpriteToggle(int x, int y, int width, int height, Texture2D source, int sourceX, int sourceY, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.sourceX = sourceX;
            this.sourceY = sourceY;
            SetParent(parent);
        }

        public void Update() {
            if (Clicked) {
                Value = !Value;
            }

        }

        Rectangle GetBounds(int num) {
            return new Rectangle(sourceX, sourceY + num * height, width, height);
        }
        public void Draw() {

            if (enabled) {
                if (Value) {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetBounds(2));
                    }
                    else {
                        DrawSprite(0, 0, GetBounds(3));
                    }
                    return;
                }
                if (IsHovered) {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetBounds(2));
                    }
                    else {
                        DrawSprite(0, 0, GetBounds(1));
                    }
                }
                else {
                    DrawSprite(0, 0, GetBounds(0));
                }
            }
            else {
                DrawSprite(0, 0, GetBounds(4));
            }

        }
    }
}
