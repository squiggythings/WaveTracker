using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WaveTracker.Rendering {
    public class Graphics {
        public static SpriteBatch batch;

        public static void DrawRect(int x, int y, int width, int height, Color color) {
            batch.Draw(App.pixel, new Rectangle(x, y, width, height), color);
        }

        public static void Write(string text, int x, int y, Color c) {
            batch.DrawString(App.font, text, new Vector2((int)x, (int)y - 5), c);
        }

        public static void WriteTwiceAsBig(string text, int x, int y, Color c) {
            batch.DrawString(App.font, text, new Vector2((int)x, (int)y - 5), c, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
        }


        public static void WriteRightJustified(string text, int x, int y, Color c) {
            batch.DrawString(App.font, text, new Vector2((int)x - Helpers.getWidthOfText(text), (int)y - 5), c);
        }

        public static void WriteMonospaced(string text, int x, int y, Color c, int width = 5) {
            foreach (char ch in text) {
                batch.DrawString(App.font, ch + "", new Vector2(x, y - 5), c);
                x += width + 1;
            }
        }

        public static void DrawSprite(Texture2D sprite, int x, int y, Rectangle bounds) {
            batch.Draw(sprite, new Rectangle(x, y, bounds.Width, bounds.Height), bounds, Color.White);
        }

        public static void DrawSprite(Texture2D sprite, int x, int y, int width, int height, Rectangle bounds) {
            batch.Draw(sprite, new Rectangle(x, y, width, height), bounds, Color.White);
        }
        public static void DrawSprite(Texture2D sprite, int x, int y, int width, int height, Rectangle bounds, Color col) {
            batch.Draw(sprite, new Rectangle(x, y, width, height), bounds, col);
        }
    }
}
