using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WaveTracker.Rendering
{
    public class Colors
    {
        public static ColorTheme theme = ColorTheme.Default;
    }

    public class ColorTheme
    {
        public Color patternText;
        public Color patternTextHighlighted;
        public Color patternTextSubHighlight;
        public int patternEmptyTextAlpha;

        public Color instrumentColumnWave;
        public Color instrumentColumnSample;
        public Color volumeColumn;
        public Color effectColumn;
        public Color effectColumnParameter;

        public Color backgroundHighlighted;
        public Color backgroundSubHighlight;
        public Color background;

        public Color rowPlaybackColor;
        public Color rowPlaybackText;
        public Color rowEditColor;
        public Color rowEditText;
        public Color rowCurrentColor;
        public Color rowCurrentText;
        public Color cursor;
        public Color rowSeparator;
        public Color selection;

        static Color AddBrightness(Color color, float amt)
        {
            return Helpers.LerpColor(color, Color.White, amt);
        }
        public static ColorTheme Default
        {

            get
            {
                ColorTheme ret = new ColorTheme();
                ret.patternText = Color.White;
                ret.patternTextHighlighted = new(202, 245, 254);
                ret.patternTextSubHighlight = new(187, 215, 254);
                ret.patternEmptyTextAlpha = 18;

                ret.instrumentColumnWave = new(90, 234, 61);
                ret.instrumentColumnSample = new(255, 153, 50);
                ret.volumeColumn = new(80, 233, 230);
                ret.effectColumn = new(255, 82, 119);
                ret.effectColumnParameter = new(255, 208, 208);

                ret.backgroundHighlighted = new(33, 40, 64);
                ret.backgroundSubHighlight = new(26, 31, 54);
                ret.background = new(20, 24, 46);
                ret.rowPlaybackColor = new(42, 29, 81);
                ret.rowPlaybackText = new(60, 37, 105);
                ret.rowEditColor = new(109, 29, 78);
                ret.rowEditText = new(162, 39, 107);
                ret.rowCurrentColor = new(27, 55, 130);
                ret.rowCurrentText = new(42, 83, 156);
                ret.cursor = new(126, 133, 168);
                ret.rowSeparator = new(49, 56, 89);
                ret.selection = new(128, 128, 255, 150);
                return ret;
            }
        }
        public static ColorTheme Famitracker
        {

            get
            {
                ColorTheme ret = new ColorTheme();

                ret.background = new(0, 0, 0);
                ret.backgroundHighlighted = new(16, 16, 0);
                ret.backgroundSubHighlight = new(32, 32, 0);

                ret.patternText = new(0, 255, 0);
                ret.patternTextHighlighted = new(240, 240, 0);
                ret.patternTextSubHighlight = new(255, 255, 96);
                ret.patternEmptyTextAlpha = 50;

                ret.instrumentColumnWave = new(128, 255, 128);
                ret.instrumentColumnSample = new(128, 255, 128);
                ret.volumeColumn = new(128, 128, 255);
                ret.effectColumn = new(255, 128, 128);
                ret.effectColumnParameter = ret.patternText;

                ret.selection = new(134, 125, 242, 150);
                ret.cursor = new(128, 128, 128);

                ret.rowCurrentColor = new(48, 32, 160);
                ret.rowCurrentText = AddBrightness(ret.rowCurrentColor, 0.17f);

                ret.rowEditColor = new(128, 32, 48);
                ret.rowEditText = AddBrightness(ret.rowEditColor, 0.17f);

                ret.rowPlaybackColor = new(80, 0, 64);
                ret.rowPlaybackText = AddBrightness(ret.rowPlaybackColor, 0.17f);

                ret.rowSeparator = new(60, 60, 60);

                return ret;
            }
        }

        public static ColorTheme OpenMPT
        {

            get
            {
                ColorTheme ret = new ColorTheme();

                ret.background = HexCode("ffffff");
                ret.backgroundHighlighted = HexCode("f2f6f2");
                ret.backgroundSubHighlight = HexCode("e0e8e0");

                ret.patternText = HexCode("000080");
                ret.patternTextHighlighted = HexCode("000080");
                ret.patternTextSubHighlight = HexCode("000080");
                ret.patternEmptyTextAlpha = 255;

                ret.instrumentColumnWave = HexCode("008080");
                ret.instrumentColumnSample = HexCode("008080");
                ret.volumeColumn = HexCode("008000");
                ret.effectColumn = HexCode("800000");
                ret.effectColumnParameter = ret.effectColumn;

                ret.selection = HexCode("8080ff");
                ret.cursor = HexCode("8080ff");

                ret.rowCurrentColor = HexCode("c0c0c0");
                ret.rowCurrentText = HexCode("000000");

                ret.rowEditColor = HexCode("c0c0c0");
                ret.rowEditText = HexCode("000000");

                ret.rowPlaybackColor = HexCode("ffff80");
                ret.rowPlaybackText = HexCode("000000");

                ret.rowSeparator = HexCode("a0a0a0");

                return ret;
            }
        }
        public static ColorTheme BambooTracker
        {

            get
            {
                ColorTheme ret = new ColorTheme();

                ret.background = HexCode("000228");
                ret.backgroundHighlighted = HexCode("1b2750");
                ret.backgroundSubHighlight = HexCode("1f327f");

                ret.patternText = HexCode("d29672");
                ret.patternTextHighlighted = HexCode("e7691b");
                ret.patternTextSubHighlight = HexCode("e7691b");
                ret.patternEmptyTextAlpha = 60;

                ret.instrumentColumnWave = HexCode("2bcbbe");
                ret.instrumentColumnSample = HexCode("2bcbbe");
                ret.volumeColumn = HexCode("e78504");
                ret.effectColumn = HexCode("27c792");
                ret.effectColumnParameter = ret.effectColumn;

                ret.selection = HexCode("0000ff80");
                ret.cursor = HexCode("88a8d3");

                ret.rowCurrentColor = HexCode("1152a8");
                ret.rowCurrentText = HexCode("ffffff");

                ret.rowEditColor = HexCode("a84b4c");
                ret.rowEditText = HexCode("ffffff");

                ret.rowPlaybackColor = HexCode("436998");
                ret.rowPlaybackText = HexCode("b4b4b4");

                ret.rowSeparator = HexCode("000000");

                return ret;
            }
        }

        public static Color HexCode(string hex)
        {
            int r = System.Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = System.Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = System.Convert.ToInt32(hex.Substring(4, 2), 16);
            int a = 255;
            if (hex.Length > 6)
                a = System.Convert.ToInt32(hex.Substring(6, 2), 16);
            return new Color(r, g, b, a);
        }
    }

    public class UIColors
    {
        /// <summary>
        /// (20, 24, 46)
        /// </summary>
        public static readonly Color black = new(20, 24, 46);
        /// <summary>
        /// (222, 223, 231)
        /// </summary>
        public static readonly Color panel = new(222, 223, 231);
        /// <summary>
        /// (43, 49, 81)
        /// </summary>
        public static readonly Color panelTitle = new(43, 49, 81);
        /// <summary>
        /// (64, 73, 115)
        /// </summary>
        public static readonly Color labelDark = new(64, 73, 115);
        /// <summary>
        /// (104, 111, 153)
        /// </summary>
        public static readonly Color label = new(104, 111, 153);
        /// <summary>
        /// (163, 167, 194)
        /// </summary>
        public static readonly Color labelLight = new(163, 167, 194);
        /// <summary>
        /// (8, 124, 232)
        /// </summary>
        public static readonly Color selection = new(8, 124, 232);
        /// <summary>
        /// (8, 124, 232)
        /// </summary>
        public static readonly Color selectionLight = new(180, 215, 248);

    }
}
