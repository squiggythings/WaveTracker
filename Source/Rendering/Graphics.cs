using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WaveTracker.Rendering {
    public static class Graphics {
        public static SpriteBatch batch;
        public static SpriteFont currentFont;
        public static SpriteFont defaultFont;
        public static SpriteFont[] highResFonts;

        public static RasterizerState scissorRasterizer;

        public static Texture2D pixel;
        public static Texture2D img;
        public static int Scale { get; set; }
        public static float fontScale = 2;
        public static float fontOffsetY = 2;

        public static void DrawRect(int x, int y, int width, int height, Color color) {
            batch.Draw(pixel, new Rectangle(x * Scale, y * Scale, width * Scale, height * Scale), color);
        }

        public static void Write(string text, int x, int y, Color c) {
            batch.DrawString(currentFont, text, new Vector2(x * Scale, (y - fontOffsetY) * Scale), c, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
        }

        public static void WriteTwiceAsBig(string text, int x, int y, Color c) {
            batch.DrawString(currentFont, text, new Vector2(x, y - fontOffsetY), c, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
        }

        public static void WriteRightJustified(string text, int x, int y, Color c) {
            batch.DrawString(currentFont, text, new Vector2((x - Helpers.GetWidthOfText(text)) * Scale, (y - fontOffsetY) * Scale), c, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
        }

        public static void WriteMonospaced(string text, int x, int y, Color c, int width = 5) {
            foreach (char ch in text) {
                batch.DrawString(currentFont, ch + "", new Vector2(x * Scale, (y - fontOffsetY) * Scale), c, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                x += width + 1;
            }
        }

        public static void DrawSprite(int x, int y, int width, int height, Rectangle bounds, Color col) {
            batch.Draw(img, new Rectangle(x * Scale, y * Scale, width * Scale, height * Scale), bounds, col);
        }
    }
}
