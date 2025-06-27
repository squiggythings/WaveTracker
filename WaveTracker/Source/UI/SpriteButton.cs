﻿using Microsoft.Xna.Framework;

namespace WaveTracker.UI {
    public class SpriteButton : Clickable {
        private int sourceX;
        private int sourceY;
        public SpriteButton(int x, int y, int width, int height, int sourceX, int sourceY, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.sourceX = sourceX;
            this.sourceY = sourceY;
            SetParent(parent);
        }

        // sprite cheatsheet
        // 0. default
        // 1. hovered
        // 2. pressed
        // 3. toggled
        // 4. disabled

        private Rectangle GetBounds(int num) {
            return new Rectangle(sourceX, sourceY + num * height, width, height);
        }

        public void Draw() {
            if (enabled) {
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
