using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

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
            Colors = new Dictionary<string, Color> {
                { "Row background (primary highlight)", Helpers.HexCodeToColor("212840") },
                { "Row background (secondary highlight)", Helpers.HexCodeToColor("1a1f36") },
                { "Row background", Helpers.HexCodeToColor("14182e") },
                { "Row text (primary highlight)", Helpers.HexCodeToColor("caf5fe") },
                { "Row text (secondary highlight)", Helpers.HexCodeToColor("bbd7fe") },
                { "Row text", Helpers.HexCodeToColor("ffffff") },
                { "Empty dashes tint", Helpers.HexCodeToColor("ffffff12") },
                { "Instrument (wave)", Helpers.HexCodeToColor("5aea3d") },
                { "Instrument (noise)", Helpers.HexCodeToColor("e535ff") },
                { "Instrument (sample)", Helpers.HexCodeToColor("ff9932") },
                { "Volume", Helpers.HexCodeToColor("50e7e5") },
                { "Effect", Helpers.HexCodeToColor("ff5277") },
                { "Effect parameter", Helpers.HexCodeToColor("ffd0d0") },
                { "Cursor", Helpers.HexCodeToColor("7e85a8") },
                { "Selection", Helpers.HexCodeToColor("8080ff96") },
                { "Current row (default)", Helpers.HexCodeToColor("1b3782") },
                { "Current row (editing)", Helpers.HexCodeToColor("6d1d4e") },
                { "Playback row", Helpers.HexCodeToColor("311f58") },
                { "Channel separator", Helpers.HexCodeToColor("313859") }
            };
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
                if (i < theme.Colors.Count - 1) {
                    str += "\n";
                }
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
        public static ColorTheme Default { get { return new ColorTheme(); } }

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
                ret["Instrument (noise)"] = Helpers.HexCodeToColor("80ff80");
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
                ret["Instrument (noise)"] = Helpers.HexCodeToColor("ff860d");
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
                ret["Instrument (noise)"] = Helpers.HexCodeToColor("008080");
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
                ret["Instrument (noise)"] = Helpers.HexCodeToColor("d4c1a0");
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
