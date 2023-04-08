using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WaveTracker.Rendering
{
    public class Graphics
    {
        public static SpriteBatch batch;

        public static void DrawRect(int x, int y, int width, int height, Color color)
        {
            batch.Draw(Game1.pixel, new Rectangle(x, y, width, height), color);
        }

        public static void Write(string text, int x, int y, Color c)
        {
            batch.DrawString(Game1.font, text, new Vector2((int)x, (int)y - 5), c);
        }

        public static void WriteMonospaced(string text, int x, int y, Color c, int width = 5)
        {
            foreach (char ch in text)
            {
                batch.DrawString(Game1.font, ch + "", new Vector2((int)x, (int)y - 5), c);
                x += width + 1;
            }
        }

        public static void WriteMonospaced(string text, int x, int y, Color c)
        {
            foreach (char ch in text)
            {
                batch.DrawString(Game1.font, ch + "", new Vector2((int)x, (int)y - 5), c);
                x += 6;
            }
        }

        public static void DrawRoundedRect(int x, int y, int width, int height, Color color)
        {
            DrawRect(x, y + 1, width, height - 2, color);
            DrawRect(x + 1, y, width - 2, height, color);
        }

        public static void DrawSprite(Texture2D sprite, int x, int y, Rectangle bounds)
        {
            batch.Draw(sprite, new Rectangle(x, y, bounds.Width, bounds.Height), bounds, Color.White);
        }

        public static void DrawSprite(Texture2D sprite, int x, int y, int width, int height, Rectangle bounds)
        {
            batch.Draw(sprite, new Rectangle(x, y, width, height), bounds, Color.White);
        }
    }
}
