using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WaveTracker.UI {
    public class SpriteToggleTwoSided : Clickable {
         int sourceX;
         int sourceY;

        public bool Value { get; set; }
        public bool ValueWasChangedInternally { get; private set; }

        public SpriteToggleTwoSided(int x, int y, int width, int height, int sourceX, int sourceY, Element parent) {
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
            if (enabled) {
                if (Clicked) {
                    Value = !Value;
                    ValueWasChangedInternally = true;
                }
                else {
                    ValueWasChangedInternally = false;
                }
            }

        }

        Rectangle GetSpriteBounds(int num) {
            return new Rectangle(sourceX, sourceY + num * height, width, height);
        }
        public void Draw() {

            if (enabled) {
                if (Value) {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetSpriteBounds(5));
                    }
                    else if (IsHovered) {
                        DrawSprite(0, 0, GetSpriteBounds(4));
                    }
                    else {
                        DrawSprite(0, 0, GetSpriteBounds(3));
                    }
                }
                else {
                    if (IsPressed) {
                        DrawSprite(0, 0, GetSpriteBounds(2));
                    }
                    else if (IsHovered) {
                        DrawSprite(0, 0, GetSpriteBounds(1));
                    }
                    else {
                        DrawSprite(0, 0, GetSpriteBounds(0));
                    }
                }
            }
            else {
                if (Value) {
                    DrawSprite(0, 0, GetSpriteBounds(6));
                }
                else {
                    DrawSprite(0, 0, GetSpriteBounds(7));
                }
            }

        }
    }
}
