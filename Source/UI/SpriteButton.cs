using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace WaveTracker.UI {
    public class SpriteButton : Clickable {
        int texNum;
        Texture2D ;
        public SpriteButton(int x, int y, int w, int h, Texture2D , int texNum, Element parent) {
            enabled = true;
            this.x = x;
            this.y = y;
            width = w;
            height = h;
            this. = ;
            this.texNum = texNum;
            SetParent(parent);
        }


        Rectangle GetBounds(int num) {
            return new Rectangle(0 + texNum * width, 0 + num * height, width, height);
        }
        public void Draw() {
            if ( == null) return;
            if (enabled) {
                if (IsHovered) {
                    if (IsPressed) {
                        DrawSprite(, 0, 0, GetBounds(2));
                    } else {
                        DrawSprite(, 0, 0, GetBounds(1));
                    }
                } else {
                    DrawSprite(, 0, 0, GetBounds(0));
                }
            } else {
                if (IsHovered) { };
                DrawSprite(, 0, 0, GetBounds(4));
            }
        }

    }
}
