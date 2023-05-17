using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WaveTracker.Rendering;

namespace WaveTracker.UI
{
    public abstract class Element
    {
        public int x, y;
        public Element parent;
        protected int MouseX
        { get { return Input.MousePositionX - (x + offX); } }
        protected int MouseY
        { get { return Input.MousePositionY - (y + offY); } }

        protected int offX { get { if (parent == null) return 0; return parent.x + parent.offX; } }
        protected int offY { get { if (parent == null) return 0; return parent.y + parent.offY; } }

        protected int globalX { get { return x + offX; } }
        protected int globalY { get { return y + offY; } }

        public bool inFocus
        {
            get
            {
                if (Input.focus == null) return true;
                if (parent == null) return Input.focus == this;
                if (parent.inFocus)
                    return true;
                return Input.focus == this;
            }
        }

        public void SetParent(Element parent)
        {
            this.parent = parent;
        }
        protected void Write(string text, int x, int y, Color color)
        {
            Graphics.Write(text, this.x + x + offX, this.y + y + offY, color);
        }
        protected void WriteTwiceAsBig(string text, int x, int y, Color c)
        {
            Graphics.WriteTwiceAsBig(text, this.x + x + offX, this.y + y + offY, c);
        }

        protected void WriteRightAlign(string text, int x, int y, Color color)
        {
            Graphics.WriteRightJustified(text, this.x + x + offX, this.y + y + offY, color);
        }
        protected void WriteMonospaced(string text, int x, int y, Color color, int width)
        {
            Graphics.WriteMonospaced(text, this.x + x + offX, this.y + y + offY, color, width);
        }
        protected void WriteMonospaced(string text, int x, int y, Color color)
        {
            Graphics.WriteMonospaced(text, this.x + x + offX, this.y + y + offY, color, 5);
        }

        protected void DrawRect(int x, int y, int width, int height, Color color)
        {
            Graphics.DrawRect(this.x + x + offX, this.y + y + offY, width, height, color);
        }

        protected void DrawRoundedRect(int x, int y, int width, int height, Color color)
        {
            Graphics.DrawRect(this.x + x + offX, this.y + y + offY + 1, width, height - 2, color);
            Graphics.DrawRect(this.x + x + offX + 1, this.y + y + offY, width - 2, height, color);
        }

        protected void DrawSprite(Texture2D sprite, int x, int y, Rectangle bounds)
        {
            Graphics.DrawSprite(sprite, this.x + x + offX, this.y + y + offY, bounds);
        }
        protected void DrawSprite(Texture2D sprite, int x, int y, int width, int height, Rectangle spriteBounds)
        {
            Graphics.DrawSprite(sprite, this.x + x + offX, this.y + y + offY, width, height, spriteBounds);
        }

        public Point globalPointToLocalPoint(Point p)
        {
            return new Point(p.X - globalX, p.Y - globalY);
        }

        public Point LastClickPos => globalPointToLocalPoint(Input.lastClickLocation);
    }
}
