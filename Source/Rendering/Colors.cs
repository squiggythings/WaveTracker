using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System;

namespace WaveTracker {

    [Serializable]
    public class ColorTheme {
        [JsonRequired]
        public Dictionary<string, Color> Colors { get; set; }

        public Color this[string name] {
            get { return Colors[name]; }
            set { Colors[name] = value; }
        }

        public ColorTheme() {
            Colors = new Dictionary<string, Color>();
            Colors.Add("Row background (primary highlight)", Helpers.HexCodeToColor("212840"));
            Colors.Add("Row background (secondary highlight)", Helpers.HexCodeToColor("1a1f36"));
            Colors.Add("Row background", Helpers.HexCodeToColor("14182e"));

            Colors.Add("Row text (primary highlight)", Helpers.HexCodeToColor("caf5fe"));
            Colors.Add("Row text (secondary highlight)", Helpers.HexCodeToColor("bbd7fe"));
            Colors.Add("Row text", Helpers.HexCodeToColor("ffffff"));
            Colors.Add("Empty dashes tint", Helpers.HexCodeToColor("ffffff12"));

            Colors.Add("Instrument (wave)", Helpers.HexCodeToColor("5aea3d"));
            Colors.Add("Instrument (sample)", Helpers.HexCodeToColor("ff9932"));
            Colors.Add("Volume", Helpers.HexCodeToColor("50e7e5"));
            Colors.Add("Effect", Helpers.HexCodeToColor("ff5277"));
            Colors.Add("Effect parameter", Helpers.HexCodeToColor("ffd0d0"));

            Colors.Add("Cursor", Helpers.HexCodeToColor("7e85a8"));
            Colors.Add("Selection", Helpers.HexCodeToColor("8080ff96"));

            Colors.Add("Current row (default)", Helpers.HexCodeToColor("1b3782"));
            Colors.Add("Current row (editing)", Helpers.HexCodeToColor("6d1d4e"));
            Colors.Add("Playback row", Helpers.HexCodeToColor("311f58"));

            Colors.Add("Channel separator", Helpers.HexCodeToColor("313859"));
        }


        public static ColorTheme Famitracker {
            get {
                ColorTheme ret = new ColorTheme();
                ret["Row background (primary highlight)"] = Helpers.HexCodeToColor("323200");
                ret["Row background (secondary highlight)"] = Helpers.HexCodeToColor("161600");
                ret["Row background"] = Helpers.HexCodeToColor("000000");

                ret["Row text (primary highlight)"] = Helpers.HexCodeToColor("f0f000");
                ret["Row text (secondary highlight)"] = Helpers.HexCodeToColor("ffff60");
                ret["Row text"] = Helpers.HexCodeToColor("00ff00");
                ret["Empty dashes tint"] = Helpers.HexCodeToColor("ffffff50");

                ret["Instrument (wave)"] = Helpers.HexCodeToColor("80ff80");
                ret["Instrument (sample)"] = Helpers.HexCodeToColor("80ff80");
                ret["Volume"] = Helpers.HexCodeToColor("8080ff");
                ret["Effect"] = Helpers.HexCodeToColor("ff8080");
                ret["Effect parameter"] = Helpers.HexCodeToColor("00ff00");

                ret["Cursor"] = Helpers.HexCodeToColor("808080");
                ret["Selection"] = Helpers.HexCodeToColor("867df296");

                ret["Current row (default)"] = Helpers.HexCodeToColor("3020a0");
                ret["Current row (editing)"] = Helpers.HexCodeToColor("802030");
                ret["Playback row"] = Helpers.HexCodeToColor("500040");

                ret["Channel separator"] = Helpers.HexCodeToColor("606060");
                return ret;
            }
        }

        public static ColorTheme Default { get { return new ColorTheme(); } }

        public static ColorTheme Neon {
            get {
                ColorTheme ret = new ColorTheme();
                ret["Row background (primary highlight)"] = Helpers.HexCodeToColor("4b1d73");
                ret["Row background (secondary highlight)"] = Helpers.HexCodeToColor("351557");
                ret["Row background"] = Helpers.HexCodeToColor("2e0845");

                ret["Row text (primary highlight)"] = Helpers.HexCodeToColor("ffffff");
                ret["Row text (secondary highlight)"] = Helpers.HexCodeToColor("ffffff");
                ret["Row text"] = Helpers.HexCodeToColor("c0c8ed");
                ret["Empty dashes tint"] = Helpers.HexCodeToColor("ffffff18");

                ret["Instrument (wave)"] = Helpers.HexCodeToColor("ffe239");
                ret["Instrument (sample)"] = Helpers.HexCodeToColor("ff860d");
                ret["Volume"] = Helpers.HexCodeToColor("00fdff");
                ret["Effect"] = Helpers.HexCodeToColor("ff7af6");
                ret["Effect parameter"] = Helpers.HexCodeToColor("97a4e2");

                ret["Cursor"] = Helpers.HexCodeToColor("756b85");
                ret["Selection"] = Helpers.HexCodeToColor("ff7a6d80");

                ret["Current row (default)"] = Helpers.HexCodeToColor("1152a8");
                ret["Current row (editing)"] = Helpers.HexCodeToColor("692744");
                ret["Playback row"] = Helpers.HexCodeToColor("000000");

                ret["Channel separator"] = Helpers.HexCodeToColor("80808040");
                return ret;
            }
        }

        /// <summary>
        /// Converts a ColorTheme to a string
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public static string CreateString(ColorTheme theme) {
            string str = "";
            for (int i = 0; i < theme.Colors.Count; i++) {
                str += theme.Colors.ElementAt(i).Key + "=" + theme.Colors.ElementAt(i).Value.GetHexCode();
                if (i < theme.Colors.Count - 1)
                    str += "\n";
            }
            return str;
        }

        /// <summary>
        /// Converts a ini formatted string into a ColorTheme
        /// </summary>
        /// <param name="fileText"></param>
        /// <returns></returns>
        public static ColorTheme FromString(string fileText) {
            ColorTheme theme = new ColorTheme();
            string[] lines = fileText.Split('\n');
            foreach (string line in lines) {
                string[] keyValuePair = line.Split('=');
                if (theme.Colors.ContainsKey(keyValuePair[0])) {
                    theme[keyValuePair[0]] = Helpers.HexCodeToColor(keyValuePair[1]);
                }
            }
            return theme;
        }

       
        //public static ColorTheme Material {

        //    get {
        //        ColorTheme ret = new ColorTheme();

        //        ret.background = Helpers.HexCodeToColor("292d3e");
        //        ret.backgroundHighlighted = Helpers.HexCodeToColor("363951");
        //        ret.backgroundSubHighlight = Helpers.HexCodeToColor("3d405f");

        //        ret.patternText = Helpers.HexCodeToColor("f78c6c");
        //        ret.patternTextHighlighted = Helpers.HexCodeToColor("ffcb6b");
        //        ret.patternTextSubHighlight = Helpers.HexCodeToColor("ffcb6b");
        //        ret.patternTextEmptyMultiply = new(255, 255, 255, 18);

        //        ret.instrumentColumnWave = Helpers.HexCodeToColor("ffffff");
        //        ret.instrumentColumnSample = Helpers.HexCodeToColor("ffffff");
        //        ret.volumeColumn = Helpers.HexCodeToColor("9cdcfe");
        //        ret.effectColumn = Helpers.HexCodeToColor("c3e88d");
        //        ret.effectColumnParameter = Helpers.HexCodeToColor("c792ea");

        //        ret.selection = Helpers.HexCodeToColor("8f8f8f50");
        //        ret.cursor = Helpers.HexCodeToColor("8f8f8f");

        //        ret.rowCursorColor = Helpers.HexCodeToColor("717171");
        //        ret.rowCursorText = Helpers.HexCodeToColor("000000");

        //        ret.rowEditColor = Helpers.HexCodeToColor("717171");
        //        ret.rowEditText = Helpers.HexCodeToColor("292d3e");

        //        ret.rowPlaybackColor = Helpers.HexCodeToColor("ffff80");
        //        ret.rowPlaybackText = Helpers.HexCodeToColor("000000");

        //        ret.channelSeparator = Helpers.HexCodeToColor("3d405f");

        //        return ret;
        //    }
        //}

        //public static ColorTheme Famitracker {

        //    get {
        //        ColorTheme ret = new ColorTheme();

        //        ret.background = new(0, 0, 0);
        //        ret.backgroundHighlighted = new(16, 16, 0);
        //        ret.backgroundSubHighlight = new(32, 32, 0);

        //        ret.patternText = new(0, 255, 0);
        //        ret.patternTextHighlighted = new(240, 240, 0);
        //        ret.patternTextSubHighlight = new(255, 255, 96);
        //        ret.patternTextEmptyMultiply = new(1, 1, 1, 50);

        //        ret.instrumentColumnWave = new(128, 255, 128);
        //        ret.instrumentColumnSample = new(128, 255, 128);
        //        ret.volumeColumn = new(128, 128, 255);
        //        ret.effectColumn = new(255, 128, 128);
        //        ret.effectColumnParameter = ret.patternText;

        //        ret.selection = new(134, 125, 242, 150);
        //        ret.cursor = new(128, 128, 128);

        //        ret.rowCursorColor = new(48, 32, 160);
        //        ret.rowCursorText = AddBrightness(ret.rowCursorColor, 0.17f);

        //        ret.rowEditColor = new(128, 32, 48);
        //        ret.rowEditText = AddBrightness(ret.rowEditColor, 0.17f);

        //        ret.rowPlaybackColor = new(80, 0, 64);
        //        ret.rowPlaybackText = AddBrightness(ret.rowPlaybackColor, 0.17f);

        //        ret.channelSeparator = new(60, 60, 60);

        //        return ret;
        //    }
        //}

        public static ColorTheme OpenMPT {

            get {
                ColorTheme ret = new ColorTheme(); 
                ret["Row background (primary highlight)"] = Helpers.HexCodeToColor("e0e8e0");
                ret["Row background (secondary highlight)"] = Helpers.HexCodeToColor("f2f6f2");
                ret["Row background"] = Helpers.HexCodeToColor("ffffff");

                ret["Row text (primary highlight)"] = Helpers.HexCodeToColor("000080");
                ret["Row text (secondary highlight)"] = Helpers.HexCodeToColor("000080");
                ret["Row text"] = Helpers.HexCodeToColor("000080");
                ret["Empty dashes tint"] = Helpers.HexCodeToColor("ffffff80");

                ret["Instrument (wave)"] = Helpers.HexCodeToColor("008080");
                ret["Instrument (sample)"] = Helpers.HexCodeToColor("008080");
                ret["Volume"] = Helpers.HexCodeToColor("008000");
                ret["Effect"] = Helpers.HexCodeToColor("800000");
                ret["Effect parameter"] = Helpers.HexCodeToColor("800000");

                ret["Cursor"] = Helpers.HexCodeToColor("8080ff");
                ret["Selection"] = Helpers.HexCodeToColor("8080ff");

                ret["Current row (default)"] = Helpers.HexCodeToColor("7cc2f1");
                ret["Current row (editing)"] = Helpers.HexCodeToColor("d692b5");
                ret["Playback row"] = Helpers.HexCodeToColor("ffff80");

                ret["Channel separator"] = Helpers.HexCodeToColor("a0a0a0");

                return ret;
            }
        }
        public static ColorTheme Fruity {

            get {
                ColorTheme ret = new ColorTheme(); 
                ret["Row background (primary highlight)"] = Helpers.HexCodeToColor("22323c");
                ret["Row background (secondary highlight)"] = Helpers.HexCodeToColor("2b3b45");
                ret["Row background"] = Helpers.HexCodeToColor("34444e");

                ret["Row text (primary highlight)"] = Helpers.HexCodeToColor("a1d6d0");
                ret["Row text (secondary highlight)"] = Helpers.HexCodeToColor("9fd3ba");
                ret["Row text"] = Helpers.HexCodeToColor("9ed1a5");
                ret["Empty dashes tint"] = Helpers.HexCodeToColor("ffffff0c");

                ret["Instrument (wave)"] = Helpers.HexCodeToColor("d4c1a0");
                ret["Instrument (sample)"] = Helpers.HexCodeToColor("d6afa2");
                ret["Volume"] = Helpers.HexCodeToColor("a8a7de");
                ret["Effect"] = Helpers.HexCodeToColor("d28bcb");
                ret["Effect parameter"] = Helpers.HexCodeToColor("ebcce7");

                ret["Cursor"] = Helpers.HexCodeToColor("81796b");
                ret["Selection"] = Helpers.HexCodeToColor("fea7592a");

                ret["Current row (default)"] = Helpers.HexCodeToColor("485066");
                ret["Current row (editing)"] = Helpers.HexCodeToColor("5d4b5b");
                ret["Playback row"] = Helpers.HexCodeToColor("414e44");

                ret["Channel separator"] = Helpers.HexCodeToColor("263640");

                return ret;
            }
        }
        //public static ColorTheme BambooTracker {

        //    get {
        //        ColorTheme ret = new ColorTheme();

        //        ret.background = Helpers.HexCodeToColor("000228");
        //        ret.backgroundHighlighted = Helpers.HexCodeToColor("1b2750");
        //        ret.backgroundSubHighlight = Helpers.HexCodeToColor("1f327f");

        //        ret.patternText = Helpers.HexCodeToColor("d29672");
        //        ret.patternTextHighlighted = Helpers.HexCodeToColor("e7691b");
        //        ret.patternTextSubHighlight = Helpers.HexCodeToColor("e7691b");
        //        ret.patternTextEmptyMultiply = new(255, 255, 255, 60);


        //        ret.instrumentColumnWave = Helpers.HexCodeToColor("2bcbbe");
        //        ret.instrumentColumnSample = Helpers.HexCodeToColor("2bcbbe");
        //        ret.volumeColumn = Helpers.HexCodeToColor("e78504");
        //        ret.effectColumn = Helpers.HexCodeToColor("27c792");
        //        ret.effectColumnParameter = ret.effectColumn;

        //        ret.selection = Helpers.HexCodeToColor("0000ff80");
        //        ret.cursor = Helpers.HexCodeToColor("88a8d3");

        //        ret.rowCursorColor = Helpers.HexCodeToColor("1152a8");
        //        ret.rowCursorText = Helpers.HexCodeToColor("ffffff");

        //        ret.rowEditColor = Helpers.HexCodeToColor("a84b4c");
        //        ret.rowEditText = Helpers.HexCodeToColor("ffffff");

        //        ret.rowPlaybackColor = Helpers.HexCodeToColor("436998");
        //        ret.rowPlaybackText = Helpers.HexCodeToColor("b4b4b4");

        //        ret.channelSeparator = Helpers.HexCodeToColor("000000");

        //        return ret;
        //    }
        //}

        //public static ColorTheme Neon {

        //    get {
        //        ColorTheme ret = new ColorTheme();

        //        ret.background = Helpers.HexCodeToColor("272336");
        //        ret.backgroundHighlighted = Helpers.HexCodeToColor("48386a");
        //        ret.backgroundSubHighlight = Helpers.HexCodeToColor("362d4e");

        //        ret.patternText = Helpers.HexCodeToColor("c0c8ed");
        //        ret.patternTextHighlighted = Helpers.HexCodeToColor("ffffff");
        //        ret.patternTextSubHighlight = Helpers.HexCodeToColor("ffffff");
        //        ret.patternTextEmptyMultiply = new(255, 255, 255, 30);


        //        ret.instrumentColumnWave = Helpers.HexCodeToColor("ffe239");
        //        ret.instrumentColumnSample = Helpers.HexCodeToColor("ff860d");
        //        ret.volumeColumn = Helpers.HexCodeToColor("00fdff");
        //        ret.effectColumn = Helpers.HexCodeToColor("ff7af6");
        //        ret.effectColumnParameter = Helpers.HexCodeToColor("97a4e2");

        //        ret.selection = Helpers.HexCodeToColor("ff7a6d80");
        //        ret.cursor = Helpers.HexCodeToColor("756b85");

        //        ret.rowCursorColor = Helpers.HexCodeToColor("1152a8");
        //        ret.rowCursorText = Helpers.HexCodeToColor("3f59a3");

        //        ret.rowEditColor = Helpers.HexCodeToColor("692744");
        //        ret.rowEditText = Helpers.HexCodeToColor("8a3366");

        //        ret.rowPlaybackColor = Helpers.HexCodeToColor("000000");
        //        ret.rowPlaybackText = Helpers.HexCodeToColor("b4b4b4");

        //        ret.channelSeparator = Helpers.HexCodeToColor("80808040");

        //        return ret;
        //    }
        //}

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
