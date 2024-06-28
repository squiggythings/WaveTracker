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
using System.Xml;

namespace WaveTracker {
    [Serializable]
    public class AppSettings {
        public AppSettings() {
            general = new General();
            files = new Files();
            patternEditor = new PatternEditor();
            keyboard = new Keyboard();
        }

        public General general;
        public class General {
            public int screenScale;
            public bool followMode;
            public bool previewNotesOnInput;
            public bool moveToNextRowAfterMultidigitInput;
        }

        public Files files;
        public class Files {
            public string defaultSpeed;
            public int defaultRowLength;
        }

        public PatternEditor patternEditor;
        public class PatternEditor {
        }

        public Keyboard keyboard;
        public class Keyboard {
            public Dictionary<string, KeyboardShortcut> shortcuts;
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
            public Keyboard() {
                shortcuts = new Dictionary<string, KeyboardShortcut>();
                for (int i = 0; i < defaultShortcuts.Count; i++) {
                    shortcuts.Add(defaultShortcuts.ElementAt(i).Key, defaultShortcuts.ElementAt(i).Value);
                }
            }
        }

        public static AppSettings Default {
            get {
                return new AppSettings();
            }
        }


    }
}
