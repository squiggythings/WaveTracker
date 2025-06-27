using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace WaveTracker.Rendering {
    public static class Graphics {
        public static SpriteBatch spriteBatch;
        public static SpriteFont defaultFont;

        public static readonly RasterizerState scissorRasterizer = new RasterizerState() { CullMode = CullMode.None, ScissorTestEnable = true };
        public static FontSystem loadedFont;
        public static SpriteFontBase customFont;

        public static Texture2D pixel;
        public static Texture2D img;
        public static int Scale { get; set; }
        public static float fontScale = 2;
        public static float fontOffsetY = 2;

        public static readonly BlendState BlendState = new BlendState {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };

        public static readonly SamplerState SamplerState = SamplerState.PointClamp;
        public static readonly DepthStencilState DepthStencilState = DepthStencilState.None;
        public static bool IsUsingCustomFont { get; private set; }

        public static void Initialize(ContentManager content, GraphicsDevice device) {
            img = content.Load<Texture2D>("img");
            pixel = new Texture2D(device, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch = new SpriteBatch(device);
            defaultFont = content.Load<SpriteFont>("wavetracker_font");

            IsUsingCustomFont = App.Settings.General.UseHighResolutionText;
            FontSystemSettings fontSettings = new FontSystemSettings();
            fontSettings.GlyphRenderer = (input, output, options) => {
                int size = options.Size.X * options.Size.Y;
                for (int i = 0; i < size; i++) {
                    int ci = i * 4;
                    output[ci] = output[ci + 1] = output[ci + 2] = 255; // set all color to white (smoother antialiasing on colored text)
                    output[ci + 3] = input[i]; // keep alpha the same
                }
            };
            loadedFont = new FontSystem(fontSettings);
            try {
                string path = Path.Combine(content.RootDirectory, "high_resolution_font.ttf");
                loadedFont.AddFont(File.ReadAllBytes(path));
            } catch {
                IsUsingCustomFont = false;
            }
            SetFont();
        }

        public static void SetFont() {
            if (IsUsingCustomFont) {
                customFont = loadedFont.GetFont(9 * App.Settings.General.ScreenScale);
                fontScale = 1;
                fontOffsetY = 1;
            }
            else {
                fontScale = App.Settings.General.ScreenScale;
                fontOffsetY = 2;
            }
        }

        public static void DrawRect(int x, int y, int width, int height, Color color) {
            spriteBatch.Draw(pixel, new Rectangle(x * Scale, y * Scale, width * Scale, height * Scale), color);
        }

        public static void Write(string text, int x, int y, Color c) {
            if (IsUsingCustomFont) {
                customFont.DrawText(spriteBatch, text, new Vector2(x * Scale, (y - fontOffsetY) * Scale), c);
            }
            else {
                spriteBatch.DrawString(defaultFont, text, new Vector2(x * Scale, (y - fontOffsetY) * Scale), c, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
            }
        }

        /// <summary>
        /// Renders multicolored text with colors for each character
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colors"></param>
        public static void Write(string text, int x, int y, Color[] colors) {

            if (IsUsingCustomFont) {
                float xpos = x;
                for (int i = 0; i < colors.Length; i++) {
                    customFont.DrawText(spriteBatch, text[i] + "", new Vector2(xpos * Scale, (y - fontOffsetY) * Scale), colors[i]);
                    xpos += customFont.MeasureString(text[i] + "").X / Scale;
                }
            }
            else {
                for (int i = 0; i < colors.Length; i++) {
                    spriteBatch.DrawString(defaultFont, text[i] + "", new Vector2(x * Scale, (y - fontOffsetY) * Scale), colors[i], 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                    x += (int)defaultFont.MeasureString(text[i] + "").X;
                }
            }
        }

        public static void WriteRightJustified(string text, int x, int y, Color c) {
            Write(text, x - Helpers.GetWidthOfText(text), y, c);
        }

        public static void WriteMonospaced(string text, int x, int y, Color c, int width = 5) {
            foreach (char ch in text) {
                Write(ch + "", x, y, c);
                x += width + 1;
            }
        }

        public static void DrawSprite(int x, int y, int width, int height, Rectangle bounds, Color col) {
            spriteBatch.Draw(img, new Rectangle(x * Scale, y * Scale, width * Scale, height * Scale), bounds, col);
        }
    }
}
