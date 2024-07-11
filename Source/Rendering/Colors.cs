using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;

namespace WaveTracker {

    public class ColorTheme {
        public Color patternText;
        public Color patternTextHighlighted;
        public Color patternTextSubHighlight;
        public Color patternTextEmptyMultiply;

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
        public Color rowCursorColor;
        public Color rowCursorText;
        public Color cursor;
        public Color rowSeparator;
        public Color selection;

        static Color AddBrightness(Color color, float amt) {
            return Helpers.LerpColor(color, Color.White, amt);
        }
        public static ColorTheme Default {

            get {
                ColorTheme ret = new ColorTheme();

                ret.background = Helpers.HexCodeToColor("14182e");
                ret.backgroundHighlighted = new(33, 40, 64);
                ret.backgroundSubHighlight = new(26, 31, 54);

                ret.patternText = Color.White;
                ret.patternTextHighlighted = new(202, 245, 254);
                ret.patternTextSubHighlight = new(187, 215, 254);
                ret.patternTextEmptyMultiply = new(255, 255, 255, 18);

                ret.instrumentColumnWave = new(90, 234, 61);
                ret.instrumentColumnSample = new(255, 153, 50);
                ret.volumeColumn = new(80, 233, 230);
                ret.effectColumn = new(255, 82, 119);
                ret.effectColumnParameter = new(255, 208, 208);

                ret.selection = new(128, 128, 255, 150);
                ret.cursor = new(126, 133, 168);

                ret.rowCursorColor = new(27, 55, 130);
                ret.rowCursorText = new(42, 83, 156);

                ret.rowEditColor = new(109, 29, 78);
                ret.rowEditText = new(162, 39, 107);

                ret.rowPlaybackColor = new(42, 29, 81);
                ret.rowPlaybackText = new(60, 37, 105);

                ret.rowSeparator = new(49, 56, 89);

                return ret;
            }
        }
        public static ColorTheme Material {

            get {
                ColorTheme ret = new ColorTheme();

                ret.background = Helpers.HexCodeToColor("292d3e");
                ret.backgroundHighlighted = Helpers.HexCodeToColor("363951");
                ret.backgroundSubHighlight = Helpers.HexCodeToColor("3d405f");

                ret.patternText = Helpers.HexCodeToColor("f78c6c");
                ret.patternTextHighlighted = Helpers.HexCodeToColor("ffcb6b");
                ret.patternTextSubHighlight = Helpers.HexCodeToColor("ffcb6b");
                ret.patternTextEmptyMultiply = new(255, 255, 255, 18);

                ret.instrumentColumnWave = Helpers.HexCodeToColor("ffffff");
                ret.instrumentColumnSample = Helpers.HexCodeToColor("ffffff");
                ret.volumeColumn = Helpers.HexCodeToColor("9cdcfe");
                ret.effectColumn = Helpers.HexCodeToColor("c3e88d");
                ret.effectColumnParameter = Helpers.HexCodeToColor("c792ea");

                ret.selection = Helpers.HexCodeToColor("8f8f8f50");
                ret.cursor = Helpers.HexCodeToColor("8f8f8f");

                ret.rowCursorColor = Helpers.HexCodeToColor("717171");
                ret.rowCursorText = Helpers.HexCodeToColor("000000");

                ret.rowEditColor = Helpers.HexCodeToColor("717171");
                ret.rowEditText = Helpers.HexCodeToColor("292d3e");

                ret.rowPlaybackColor = Helpers.HexCodeToColor("ffff80");
                ret.rowPlaybackText = Helpers.HexCodeToColor("000000");

                ret.rowSeparator = Helpers.HexCodeToColor("3d405f");

                return ret;
            }
        }

        public static ColorTheme Famitracker {

            get {
                ColorTheme ret = new ColorTheme();

                ret.background = new(0, 0, 0);
                ret.backgroundHighlighted = new(16, 16, 0);
                ret.backgroundSubHighlight = new(32, 32, 0);

                ret.patternText = new(0, 255, 0);
                ret.patternTextHighlighted = new(240, 240, 0);
                ret.patternTextSubHighlight = new(255, 255, 96);
                ret.patternTextEmptyMultiply = new(1, 1, 1, 50);

                ret.instrumentColumnWave = new(128, 255, 128);
                ret.instrumentColumnSample = new(128, 255, 128);
                ret.volumeColumn = new(128, 128, 255);
                ret.effectColumn = new(255, 128, 128);
                ret.effectColumnParameter = ret.patternText;

                ret.selection = new(134, 125, 242, 150);
                ret.cursor = new(128, 128, 128);

                ret.rowCursorColor = new(48, 32, 160);
                ret.rowCursorText = AddBrightness(ret.rowCursorColor, 0.17f);

                ret.rowEditColor = new(128, 32, 48);
                ret.rowEditText = AddBrightness(ret.rowEditColor, 0.17f);

                ret.rowPlaybackColor = new(80, 0, 64);
                ret.rowPlaybackText = AddBrightness(ret.rowPlaybackColor, 0.17f);

                ret.rowSeparator = new(60, 60, 60);

                return ret;
            }
        }

        public static ColorTheme OpenMPT {

            get {
                ColorTheme ret = new ColorTheme();

                ret.background = Helpers.HexCodeToColor("ffffff");
                ret.backgroundHighlighted = Helpers.HexCodeToColor("f2f6f2");
                ret.backgroundSubHighlight = Helpers.HexCodeToColor("e0e8e0");

                ret.patternText = Helpers.HexCodeToColor("000080");
                ret.patternTextHighlighted = Helpers.HexCodeToColor("000080");
                ret.patternTextSubHighlight = Helpers.HexCodeToColor("000080");
                ret.patternTextEmptyMultiply = new(255, 255, 255, 255);

                ret.instrumentColumnWave = Helpers.HexCodeToColor("008080");
                ret.instrumentColumnSample = Helpers.HexCodeToColor("008080");
                ret.volumeColumn = Helpers.HexCodeToColor("008000");
                ret.effectColumn = Helpers.HexCodeToColor("800000");
                ret.effectColumnParameter = ret.effectColumn;

                ret.selection = Helpers.HexCodeToColor("8080ff");
                ret.cursor = Helpers.HexCodeToColor("8080ff");

                ret.rowCursorColor = Helpers.HexCodeToColor("c0c0c0");
                ret.rowCursorText = Helpers.HexCodeToColor("000000");

                ret.rowEditColor = Helpers.HexCodeToColor("c0c0c0");
                ret.rowEditText = Helpers.HexCodeToColor("000000");

                ret.rowPlaybackColor = Helpers.HexCodeToColor("ffff80");
                ret.rowPlaybackText = Helpers.HexCodeToColor("000000");

                ret.rowSeparator = Helpers.HexCodeToColor("a0a0a0");

                return ret;
            }
        }
        public static ColorTheme BambooTracker {

            get {
                ColorTheme ret = new ColorTheme();

                ret.background = Helpers.HexCodeToColor("000228");
                ret.backgroundHighlighted = Helpers.HexCodeToColor("1b2750");
                ret.backgroundSubHighlight = Helpers.HexCodeToColor("1f327f");

                ret.patternText = Helpers.HexCodeToColor("d29672");
                ret.patternTextHighlighted = Helpers.HexCodeToColor("e7691b");
                ret.patternTextSubHighlight = Helpers.HexCodeToColor("e7691b");
                ret.patternTextEmptyMultiply = new(255, 255, 255, 60);


                ret.instrumentColumnWave = Helpers.HexCodeToColor("2bcbbe");
                ret.instrumentColumnSample = Helpers.HexCodeToColor("2bcbbe");
                ret.volumeColumn = Helpers.HexCodeToColor("e78504");
                ret.effectColumn = Helpers.HexCodeToColor("27c792");
                ret.effectColumnParameter = ret.effectColumn;

                ret.selection = Helpers.HexCodeToColor("0000ff80");
                ret.cursor = Helpers.HexCodeToColor("88a8d3");

                ret.rowCursorColor = Helpers.HexCodeToColor("1152a8");
                ret.rowCursorText = Helpers.HexCodeToColor("ffffff");

                ret.rowEditColor = Helpers.HexCodeToColor("a84b4c");
                ret.rowEditText = Helpers.HexCodeToColor("ffffff");

                ret.rowPlaybackColor = Helpers.HexCodeToColor("436998");
                ret.rowPlaybackText = Helpers.HexCodeToColor("b4b4b4");

                ret.rowSeparator = Helpers.HexCodeToColor("000000");

                return ret;
            }
        }

        public static ColorTheme Neon {

            get {
                ColorTheme ret = new ColorTheme();

                ret.background = Helpers.HexCodeToColor("272336");
                ret.backgroundHighlighted = Helpers.HexCodeToColor("48386a");
                ret.backgroundSubHighlight = Helpers.HexCodeToColor("362d4e");

                ret.patternText = Helpers.HexCodeToColor("c0c8ed");
                ret.patternTextHighlighted = Helpers.HexCodeToColor("ffffff");
                ret.patternTextSubHighlight = Helpers.HexCodeToColor("ffffff");
                ret.patternTextEmptyMultiply = new(255, 255, 255, 30);


                ret.instrumentColumnWave = Helpers.HexCodeToColor("ffe239");
                ret.instrumentColumnSample = Helpers.HexCodeToColor("ff860d");
                ret.volumeColumn = Helpers.HexCodeToColor("00fdff");
                ret.effectColumn = Helpers.HexCodeToColor("ff7af6");
                ret.effectColumnParameter = Helpers.HexCodeToColor("97a4e2");

                ret.selection = Helpers.HexCodeToColor("ff7a6d80");
                ret.cursor = Helpers.HexCodeToColor("756b85");

                ret.rowCursorColor = Helpers.HexCodeToColor("1152a8");
                ret.rowCursorText = Helpers.HexCodeToColor("3f59a3");

                ret.rowEditColor = Helpers.HexCodeToColor("692744");
                ret.rowEditText = Helpers.HexCodeToColor("8a3366");

                ret.rowPlaybackColor = Helpers.HexCodeToColor("000000");
                ret.rowPlaybackText = Helpers.HexCodeToColor("b4b4b4");

                ret.rowSeparator = Helpers.HexCodeToColor("80808040");

                return ret;
            }
        }

    }

    public class UIColors {
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
        /// (180, 215, 248)
        /// </summary>
        public static readonly Color selectionLight = new(180, 215, 248);

    }
}
