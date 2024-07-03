using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Text.Json;

namespace WaveTracker {
    public class AppSettings {
        public _General General { get; set; }
        public _Appearance Appearance { get; set; }
        public _PatternEditor PatternEditor { get; set; }
        public _SamplesWaves SamplesWaves { get; set; }
        public _MIDI MIDI { get; set; }
        public _Audio Audio { get; set; }
        public _Visualizer Visualizer { get; set; }
        public _Keyboard Keyboard { get; set; }

        public AppSettings() {
            General = new _General();
            Appearance = new _Appearance();
            PatternEditor = new _PatternEditor();
            Audio = new _Audio();
            Keyboard = new _Keyboard();
        }
        public class _General {
            public int ScreenScale { get; set; } = 1;
            public int OscilloscopeMode { get; set; } = 1;
            public int MeterDecayRate { get; set; } = 3;
            public int MeterColorMode { get; set; } = 1;
            public bool FlashMeterRedWhenClipping { get; set; } = true;
            public static string DefaultTicksPerRow { get; set; } = "4";
            public static int DefaultRowsPerFrame { get; set; } = 64;
            public static int DefaultNumberOfChannels { get; set; } = 12;
            public static string DefaultArtistName { get; set; } = "";
        }

        public class _Appearance {
            public ColorTheme Theme { get; set; } = ColorTheme.Default;
        }

        public class _PatternEditor {
            public MoveToNextRowBehavior StepAfterNumericInput { get; set; } = MoveToNextRowBehavior.Always;
            public bool FollowCursorDuringPlayback { get; set; } = true;
            public bool PreviewNotesOnInput { get; set; } = true;
            public bool ShowPreviousNextPatterns { get; set; } = true;

            public bool WrapCursor { get; set; } = true;
            public bool WrapCursorAcrossFrames { get; set; } = true;

        }

        public class _SamplesWaves {
        }

        public class _MIDI {
        }


        public class _Audio {
            public int OutputDevice { get; set; } = 0;
            public int MasterVolume { get; set; } = 100;
            public int Oversampling { get; set; } = 1;
        }
        public class _Visualizer {
        }


        public class _Keyboard {
            public Dictionary<string, KeyboardShortcut> Shortcuts { get; set; }

            public KeyboardShortcut this[string section, string name] {
                get {
                    return Shortcuts[section + "\\" + name];
                }
            }

            public readonly Dictionary<string, KeyboardShortcut> defaultShortcuts = new Dictionary<string, KeyboardShortcut>() {
                {"General\\Increase octave", new KeyboardShortcut(Keys.OemOpenBrackets) },
                {"General\\Decrease octave", new KeyboardShortcut(Keys.OemCloseBrackets) },
                {"General\\Play/Stop", new KeyboardShortcut(Keys.Enter) },
                {"General\\Play from beginning", new KeyboardShortcut(Keys.F5) },
                {"General\\Play from cursor", new KeyboardShortcut(Keys.Enter, KeyModifier.Alt) },
                {"General\\Play row", new KeyboardShortcut(Keys.Enter, KeyModifier.Ctrl) },
                {"General\\Stop", new KeyboardShortcut(Keys.F8) },
                {"General\\Toggle Edit mode", new KeyboardShortcut(Keys.Space) },
                {"General\\Increase step", new KeyboardShortcut() },
                {"General\\Decrease step", new KeyboardShortcut() },
                {"General\\Next instrument", new KeyboardShortcut(Keys.Down, KeyModifier.Ctrl) },
                {"General\\Previous instrument", new KeyboardShortcut(Keys.Up, KeyModifier.Ctrl) },
                {"General\\Follow mode", new KeyboardShortcut(Keys.Scroll) },
                {"General\\Module settings", new KeyboardShortcut() },
                {"General\\Edit wave", new KeyboardShortcut() },
                {"General\\Edit instrument", new KeyboardShortcut() },


                {"Frame\\Previous Frame", new KeyboardShortcut(Keys.Left, KeyModifier.Ctrl) },
                {"Frame\\Next Frame", new KeyboardShortcut(Keys.Right, KeyModifier.Ctrl) },
                {"Frame\\Duplicate Frame", new KeyboardShortcut(Keys.D, KeyModifier.Ctrl) },
                {"Frame\\Remove Frame", new KeyboardShortcut() },
                {"Frame\\Increase pattern value", new KeyboardShortcut() },
                {"Frame\\Decrease pattern value", new KeyboardShortcut() },


                {"Edit\\Undo", new KeyboardShortcut(Keys.Z, KeyModifier.Ctrl) },
                {"Edit\\Redo", new KeyboardShortcut(Keys.Y, KeyModifier.Ctrl) },
                {"Edit\\Cut", new KeyboardShortcut(Keys.X, KeyModifier.Ctrl) },
                {"Edit\\Copy", new KeyboardShortcut(Keys.C, KeyModifier.Ctrl) },
                {"Edit\\Paste", new KeyboardShortcut(Keys.V, KeyModifier.Ctrl) },
                {"Edit\\Paste and mix", new KeyboardShortcut(Keys.M, KeyModifier.Ctrl) },
                {"Edit\\Delete", new KeyboardShortcut(Keys.Delete) },

                {"Pattern\\Interpolate", new KeyboardShortcut(Keys.G, KeyModifier.Ctrl) },
                {"Pattern\\Reverse", new KeyboardShortcut(Keys.R, KeyModifier.Ctrl) },
                {"Pattern\\Replace instrument", new KeyboardShortcut(Keys.S, KeyModifier.Alt) },
                {"Pattern\\Transpose: note down", new KeyboardShortcut(Keys.F1, KeyModifier.Ctrl) },
                {"Pattern\\Transpose: note up", new KeyboardShortcut(Keys.F2, KeyModifier.Ctrl) },
                {"Pattern\\Transpose: octave down", new KeyboardShortcut(Keys.F3, KeyModifier.Ctrl) },
                {"Pattern\\Transpose: octave up", new KeyboardShortcut(Keys.F4, KeyModifier.Ctrl) },
                {"Pattern\\Decrease values", new KeyboardShortcut(Keys.F1, KeyModifier.Shift) },
                {"Pattern\\Increase values", new KeyboardShortcut(Keys.F2, KeyModifier.Shift) },
                {"Pattern\\Coarse decrease values", new KeyboardShortcut(Keys.F3, KeyModifier.Shift) },
                {"Pattern\\Coarse increase values", new KeyboardShortcut(Keys.F4, KeyModifier.Shift) },


                {"Piano\\Note off", new KeyboardShortcut(Keys.D1) },
                {"Piano\\Note release", new KeyboardShortcut(Keys.OemPlus) },
                {"Piano\\Lower C-1", new KeyboardShortcut(Keys.Z) },
                {"Piano\\Lower C#1", new KeyboardShortcut(Keys.S) },
                {"Piano\\Lower D-1", new KeyboardShortcut(Keys.X) },
                {"Piano\\Lower D#1", new KeyboardShortcut(Keys.D) },
                {"Piano\\Lower E-1", new KeyboardShortcut(Keys.C) },
                {"Piano\\Lower F-1", new KeyboardShortcut(Keys.V) },
                {"Piano\\Lower F#1", new KeyboardShortcut(Keys.G) },
                {"Piano\\Lower G-1", new KeyboardShortcut(Keys.B) },
                {"Piano\\Lower G#1", new KeyboardShortcut(Keys.H) },
                {"Piano\\Lower A-1", new KeyboardShortcut(Keys.N) },
                {"Piano\\Lower A#1", new KeyboardShortcut(Keys.J) },
                {"Piano\\Lower B-1", new KeyboardShortcut(Keys.M) },
                {"Piano\\Lower C-2", new KeyboardShortcut(Keys.OemComma) },
                {"Piano\\Lower C#2", new KeyboardShortcut(Keys.L) },
                {"Piano\\Lower D-2", new KeyboardShortcut(Keys.OemPeriod) },
                {"Piano\\Lower D#2", new KeyboardShortcut(Keys.OemSemicolon) },
                {"Piano\\Lower E-2", new KeyboardShortcut(Keys.OemQuestion) },
                {"Piano\\Upper C-2", new KeyboardShortcut(Keys.Q) },
                {"Piano\\Upper C#2", new KeyboardShortcut(Keys.D2) },
                {"Piano\\Upper D-2", new KeyboardShortcut(Keys.W) },
                {"Piano\\Upper D#2", new KeyboardShortcut(Keys.D3) },
                {"Piano\\Upper E-2", new KeyboardShortcut(Keys.E) },
                {"Piano\\Upper F-2", new KeyboardShortcut(Keys.R) },
                {"Piano\\Upper F#2", new KeyboardShortcut(Keys.D5) },
                {"Piano\\Upper G-2", new KeyboardShortcut(Keys.T) },
                {"Piano\\Upper G#2", new KeyboardShortcut(Keys.D6) },
                {"Piano\\Upper A-2", new KeyboardShortcut(Keys.Y) },
                {"Piano\\Upper A#2", new KeyboardShortcut(Keys.D7) },
                {"Piano\\Upper B-2", new KeyboardShortcut(Keys.U) },
                {"Piano\\Upper C-3", new KeyboardShortcut(Keys.I) },
                {"Piano\\Upper C#3", new KeyboardShortcut(Keys.D9) },
                {"Piano\\Upper D-3", new KeyboardShortcut(Keys.O) },
                {"Piano\\Upper D#3", new KeyboardShortcut(Keys.D0) },
                {"Piano\\Upper E-3", new KeyboardShortcut(Keys.P) },
            };
            public _Keyboard() {
                Shortcuts = new Dictionary<string, KeyboardShortcut>();
                for (int i = 0; i < defaultShortcuts.Count; i++) {
                    Shortcuts.Add(defaultShortcuts.ElementAt(i).Key, defaultShortcuts.ElementAt(i).Value);
                }
            }
        }

        public static AppSettings Default {
            get {
                return new AppSettings();
            }
        }

        /// <summary>
        /// How the cursor should respond when inputting in a numeric cell
        /// </summary>
        public enum MoveToNextRowBehavior {
            /// <summary>
            /// Always step after any input
            /// </summary>
            Always,
            /// <summary>
            /// Step only when the cursor reaches the end of the cell
            /// </summary>
            AfterCell,
            /// <summary>
            /// Step when the cursor gets to the end of a cell, but treat effects as one cell
            /// </summary>
            AfterCellIncludingEffect,
            /// <summary>
            /// Don't step automatically; The user must navigate to the next cell manually.
            /// </summary>
            Never
        }

        /// <summary>
        /// Writes this AppSettings to a json formatted file at <c>path/fileName</c>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        public void WriteToDisk(string path, string fileName) {
            Path.Combine(path, fileName);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, fileName))) {

                JsonSerializer.Serialize(outputFile, typeof(AppSettings));

            }
        }

        /// <summary>
        /// Returns an AppSettings instance read from the given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static AppSettings ReadFromDisk(string path, string fileName) {
            Path.Combine(path, fileName);
            string jsonString = File.ReadAllText(Path.Combine(path, fileName));
            AppSettings ret = JsonSerializer.Deserialize<AppSettings>(jsonString);
            return ret;
        }
    }
}
