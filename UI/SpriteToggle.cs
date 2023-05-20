using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace WaveTracker.UI
{
    public class SpriteToggle : Clickable
    {
        int texNum;
        Texture2D source;

        public bool Value { get; set; }

        public SpriteToggle(int x, int y, int w, int h, Texture2D source, int texNum, Element parent)
        {
            enabled = true;
            this.x = x;
            this.y = y;
            width = w;
            height = h;
            this.source = source;
            this.texNum = texNum;
            SetParent(parent);
        }

        public void Update()
        {
            if (Clicked)
            {
                Value = !Value;
            }

        }

        Rectangle GetBounds(int num)
        {
            return new Rectangle(0 + texNum * width, 0 + num * height, width, height);
        }
        public void Draw()
        {
            if (source == null) return;

            if (enabled)
            {
                if (Value)
                {
                    if (IsPressed)
                    {
                        DrawSprite(source, 0, 0, GetBounds(2));
                    }
                    else
                    {
                        DrawSprite(source, 0, 0, GetBounds(3));
                    }
                    return;
                }
                if (IsHovered)
                {
                    if (IsPressed)
                    {
                        DrawSprite(source, 0, 0, GetBounds(2));
                    }
                    else
                    {
                        DrawSprite(source, 0, 0, GetBounds(1));
                    }
                }
                else
                {
                    DrawSprite(source, 0, 0, GetBounds(0));
                }
            }
            else
            {
                DrawSprite(source, 0, 0, GetBounds(4));
            }

        }
    }
}
