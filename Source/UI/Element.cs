using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WaveTracker.Rendering;

namespace WaveTracker.UI {
    public abstract class Element {
        public int x, y;
        public Element parent;
        protected int MouseX { get { return Input.MousePositionX - (x + OffX); } }
        protected int MouseY { get { return Input.MousePositionY - (y + OffY); } }

        protected int OffX { get { return parent == null ? 0 : parent.x + parent.OffX; } }
        protected int OffY { get { return parent == null ? 0 : parent.y + parent.OffY; } }

        protected int GlobalX { get { return x + OffX; } }
        protected int GlobalY { get { return y + OffY; } }

        public bool InFocus {
            get {
                return Input.focus == null || (parent == null ? Input.focus == this : parent.InFocus || Input.focus == this);
            }
        }

        public bool IsMeOrAParent(Element element) {
            if (parent == null) {
                return element == this || element == null;
            }
            else {
                return element == this || parent.IsMeOrAParent(element);
            }
        }

        public void SetParent(Element parent) {
            this.parent = parent;
        }
        protected void Write(string text, int x, int y, Color color) {
            Graphics.Write(text, this.x + x + OffX, this.y + y + OffY, color);
        }

        protected void DebugWrite(string text, int x, int y) {
            DrawRect(x, y, Helpers.GetWidthOfText(text), 10, new Color(0, 0, 0, 128));
            Write(text, x, y, Color.White);
        }
        /// <summary>
        /// Renders text in multicolor, <c>colors</c> indicates a color for each character.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colors"></param>
        protected void WriteWithHighlight(string text, int x, int y, Color[] colors) {
            Graphics.Write(text, this.x + x + OffX, this.y + y + OffY, colors);
        }

        protected void WriteMultiline(string text, int x, int y, int width, Color color, int lineSpacing = 10) {
            string str = "";
            string[] words = text.Split(' ');
            int w = 0;
            foreach (string word in words) {
                int wordWidth = Helpers.GetWidthOfText(word + " ");
                if (w + wordWidth > width || word == "\n") {
                    if (word == "\n") {
                        str += "\n";
                        w = 0;
                    }
                    else if (wordWidth > width) {
                        string subWord = "";
                        int widthOffset = w;

                        foreach (var c in word) {
                            if (Helpers.GetWidthOfText(subWord + c + " ") + widthOffset > width) {
                                str += subWord + "\n";
                                subWord = "";
                                widthOffset = 0;
                            }

                            subWord += c;
                        }

                        str += subWord + " ";
                        w = Helpers.GetWidthOfText(subWord + " ");
                    }
                    else {
                        str += "\n" + word + " ";
                        w = wordWidth;
                    }
                }
                else {
                    str += word + " ";
                    w += wordWidth;
                }
            }
            string[] lines = str.Split('\n');
            foreach (string line in lines) {
                Write(line, x, y, color);
                y += lineSpacing;
            }
        }

        protected void WriteRightAlign(string text, int x, int y, Color color) {
            Graphics.WriteRightJustified(text, this.x + x + OffX, this.y + y + OffY, color);
        }

        protected void WriteCenter(string text, int x, int y, Color color) {
            Write(text, x - Helpers.GetWidthOfText(text) / 2, y, color);
        }

        protected void WriteMonospaced(string text, int x, int y, Color color, int width = 4) {
            Graphics.WriteMonospaced(text, this.x + x + OffX, this.y + y + OffY, color, width);
        }

        protected void DrawRect(int x, int y, int width, int height, Color color) {
            Graphics.DrawRect(this.x + x + OffX, this.y + y + OffY, width, height, color);
        }

        protected void DrawRoundedRect(int x, int y, int width, int height, Color color) {
            Graphics.DrawRect(this.x + x + OffX, this.y + y + OffY + 1, width, height - 2, color);
            Graphics.DrawRect(this.x + x + OffX + 1, this.y + y + OffY, width - 2, height, color);
        }

        protected void DrawSprite(int x, int y, Rectangle spriteBounds) {
            Graphics.DrawSprite(this.x + x + OffX, this.y + y + OffY, spriteBounds.Width, spriteBounds.Height, spriteBounds, Color.White);
        }
        protected void DrawSprite(int x, int y, Rectangle spriteBounds, Color color) {
            Graphics.DrawSprite(this.x + x + OffX, this.y + y + OffY, spriteBounds.Width, spriteBounds.Height, spriteBounds, color);
        }
        protected void DrawSprite(int x, int y, int width, int height, Rectangle spriteBounds) {
            Graphics.DrawSprite(this.x + x + OffX, this.y + y + OffY, width, height, spriteBounds, Color.White);
        }
        protected void DrawSprite(int x, int y, int width, int height, Rectangle spriteBounds, Color col) {
            Graphics.DrawSprite(this.x + x + OffX, this.y + y + OffY, width, height, spriteBounds, col);
        }

        /// <summary>
        /// Sets drawing to be inside a rectangular mask, anything outside will be clipped.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected void StartRectangleMask(int x, int y, int width, int height) {
            // end the batch before this
            Graphics.spriteBatch.End();

            // begin a new batch using the scissor test masking feature
            Graphics.spriteBatch.Begin(SpriteSortMode.Deferred,
                Graphics.BlendState,
                Graphics.SamplerState,
                Graphics.DepthStencilState,
                Graphics.scissorRasterizer);

            // set the scissor rectangle to the bounds of this element on the screen, anything drawn beyond it will be clipped
            Graphics.spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle((this.x + x + OffX) * Graphics.Scale, (this.y + y + OffY) * Graphics.Scale, width * Graphics.Scale, height * Graphics.Scale);
        }

        /// <summary>
        /// Ends rectangular clipping and resumes regular drawing
        /// </summary>
        protected static void EndRectangleMask() {
            // end the clipped batch
            Graphics.spriteBatch.End();

            // begin another batch without scissor clipping to resume regular drawing
            Graphics.spriteBatch.Begin(SpriteSortMode.Deferred,
                Graphics.BlendState,
                Graphics.SamplerState,
                Graphics.DepthStencilState,
                RasterizerState.CullNone);
        }

        public Point GlobalPointToLocalPoint(Point p) {
            return new Point(p.X - GlobalX, p.Y - GlobalY);
        }

        public Point LastClickPos {
            get {
                return GlobalPointToLocalPoint(Input.LastClickLocation);
            }
        }
    }
}
