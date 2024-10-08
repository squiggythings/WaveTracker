﻿using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WaveTracker.Audio;

namespace WaveTracker {
    public class SettingsProfile {
        public CategoryGeneral General { get; set; }
        public CategoryFiles Files { get; set; }
        public CategoryAppearance Colors { get; set; }
        public CategoryPatternEditor PatternEditor { get; set; }
        public CategorySamplesWaves SamplesWaves { get; set; }
        public CategoryMIDI MIDI { get; set; }
        public CategoryAudio Audio { get; set; }
        public CategoryVisualizer Visualizer { get; set; }
        public CategoryKeyboard Keyboard { get; set; }

        public SettingsProfile() {
            General = new CategoryGeneral();
            Files = new CategoryFiles();
            Colors = new CategoryAppearance();
            PatternEditor = new CategoryPatternEditor();
            SamplesWaves = new CategorySamplesWaves();
            MIDI = new CategoryMIDI();
            Audio = new CategoryAudio();
            Visualizer = new CategoryVisualizer();
            Keyboard = new CategoryKeyboard();

        }
        public class CategoryGeneral {
            public int ScreenScale { get; set; } = 2;
            public bool UseHighResolutionText { get; set; } = false;

            public int OscilloscopeMode { get; set; } = 2;
            public int MeterDecayRate { get; set; } = 2;
            public int MeterColorMode { get; set; } = 1;
            public bool FlashMeterRedWhenClipping { get; set; } = true;
        }

        public class CategoryFiles {
            public string DefaultTicksPerRow { get; set; } = "4";
            public int DefaultRowsPerFrame { get; set; } = 64;
            public int DefaultNumberOfChannels { get; set; } = 12;
            public string DefaultAuthorName { get; set; } = "";
            public int DefaultRowPrimaryHighlight { get; set; } = 16;
            public int DefaultRowSecondaryHighlight { get; set; } = 4;
        }

        public class CategoryAppearance {
            public ColorTheme Theme { get; set; } = ColorTheme.Default;
        }

        public class CategoryPatternEditor {
            public bool ShowRowNumbersInHex { get; set; } = false;
            public bool ShowNoteOffAndReleaseAsText { get; set; } = false;
            public bool FadeVolumeColumn { get; set; } = true;
            public bool ShowPreviousNextFrames { get; set; } = true;
            public bool IgnoreStepWhenMoving { get; set; } = true;
            public MoveToNextRowBehavior StepAfterNumericInput { get; set; } = MoveToNextRowBehavior.Always;
            public bool WrapCursorHorizontally { get; set; } = true;
            public bool KeyRepeat { get; set; } = true;
            public int PageJumpAmount { get; set; } = 4;
            public bool RestoreChannelState { get; set; } = true;

        }

        public class CategorySamplesWaves {

            public bool AutomaticallyNormalizeSamplesOnImport { get; set; } = true;
            public bool AutomaticallyTrimSamples { get; set; } = true;
            public bool PreviewSamplesInBrowser { get; set; } = true;
            public int DefaultSampleBaseKey { get; set; } = 60;
            public bool IncludeSamplesInVisualizerByDefault { get; set; } = false;
            public ResamplingMode DefaultResampleModeWave { get; set; } = ResamplingMode.Mix;
            public ResamplingMode DefaultResampleModeSample { get; set; } = ResamplingMode.Linear;

        }

        public class CategoryMIDI {
            public string InputDevice { get; set; } = "(none)";
            public bool RecordNoteVelocity { get; set; } = true;
            public int MIDITranspose { get; set; } = 0;
            public bool ApplyOctaveTranspose { get; set; } = false;
            public bool UseProgramChangeToSelectInstrument { get; set; } = true;
            public bool ReceivePlayStopMessages { get; set; } = true;
        }

        public class CategoryAudio {
            public string OutputDevice { get; set; } = "(default)";

            public int MasterVolume { get; set; } = 100;
            public SampleRate SampleRate { get; set; } = SampleRate._44100;
            public int Oversampling { get; set; } = 2;
            public int DesiredLatency { get; set; } = 120;
        }
        public class CategoryVisualizer {
            public int PianoSpeed { get; set; } = 8;
            public bool ChangeNoteOpacityByVolume { get; set; } = true;
            public bool ChangeNoteWidthByVolume { get; set; } = true;
            public bool HighlightPressedKeys { get; set; } = true;
            public int OscilloscopeZoom { get; set; } = 100;
            public bool OscilloscopeColorfulWaves { get; set; } = true;
            public int OscilloscopeThickness { get; set; } = 1;
            public int OscilloscopeCrosshairs { get; set; } = 1;
            public bool OscilloscopeBorders { get; set; } = false;
            public bool DrawInHighResolution { get; set; } = false;
        }

        public class CategoryKeyboard {
            public Dictionary<string, KeyboardShortcut> Shortcuts { get; set; }

            public static readonly Dictionary<string, KeyboardShortcut> defaultShortcuts = new Dictionary<string, KeyboardShortcut>() {
                {"General\\Play/Stop", new KeyboardShortcut(Keys.Enter) },
                {"General\\Play from beginning", new KeyboardShortcut(Keys.F5) },
                {"General\\Play and loop pattern", new KeyboardShortcut(Keys.Enter, KeyModifier.Shift) },
                {"General\\Play from cursor", new KeyboardShortcut(Keys.Enter, KeyModifier.Alt) },
                {"General\\Play row", new KeyboardShortcut(Keys.Enter, KeyModifier.Ctrl) },
                {"General\\Stop", new KeyboardShortcut(Keys.F8) },
                {"General\\Toggle edit mode", new KeyboardShortcut(Keys.Space) },
                {"General\\Increase octave", new KeyboardShortcut(Keys.OemCloseBrackets) },
                {"General\\Decrease octave", new KeyboardShortcut(Keys.OemOpenBrackets) },
                {"General\\Set octave 0", new KeyboardShortcut(Keys.NumPad0) },
                {"General\\Set octave 1", new KeyboardShortcut(Keys.NumPad1) },
                {"General\\Set octave 2", new KeyboardShortcut(Keys.NumPad2) },
                {"General\\Set octave 3", new KeyboardShortcut(Keys.NumPad3) },
                {"General\\Set octave 4", new KeyboardShortcut(Keys.NumPad4) },
                {"General\\Set octave 5", new KeyboardShortcut(Keys.NumPad5) },
                {"General\\Set octave 6", new KeyboardShortcut(Keys.NumPad6) },
                {"General\\Set octave 7", new KeyboardShortcut(Keys.NumPad7) },
                {"General\\Set octave 8", new KeyboardShortcut(Keys.NumPad8) },
                {"General\\Set octave 9", new KeyboardShortcut(Keys.NumPad9) },
                {"General\\Increase step", new KeyboardShortcut() },
                {"General\\Decrease step", new KeyboardShortcut() },
                {"General\\Next instrument", new KeyboardShortcut(Keys.Down, KeyModifier.Ctrl) },
                {"General\\Previous instrument", new KeyboardShortcut(Keys.Up, KeyModifier.Ctrl) },
                {"General\\Follow mode", new KeyboardShortcut(Keys.Scroll) },
                {"General\\Module settings", new KeyboardShortcut(Keys.P, KeyModifier.Alt) },
                {"General\\Edit wave", new KeyboardShortcut(Keys.W, KeyModifier.Ctrl) },
                {"General\\Edit instrument", new KeyboardShortcut(Keys.I, KeyModifier.Ctrl) },
                {"General\\Rename instrument", new KeyboardShortcut(Keys.R, KeyModifier.CtrlShift) },
                {"General\\Move instrument up", new KeyboardShortcut(Keys.Up, KeyModifier.CtrlShift) },
                {"General\\Move instrument down", new KeyboardShortcut(Keys.Down, KeyModifier.CtrlShift) },
                {"General\\Toggle visualizer", new KeyboardShortcut(Keys.V, KeyModifier.Alt) },
                {"General\\Toggle channel", new KeyboardShortcut(Keys.F9, KeyModifier.Alt) },
                {"General\\Solo channel", new KeyboardShortcut(Keys.F10, KeyModifier.Alt) },
                {"General\\Reset audio", new KeyboardShortcut(Keys.F12, KeyModifier.None) },

                {"Frame\\Previous frame", new KeyboardShortcut(Keys.Left, KeyModifier.Ctrl) },
                {"Frame\\Next frame", new KeyboardShortcut(Keys.Right, KeyModifier.Ctrl) },
                {"Frame\\Duplicate frame", new KeyboardShortcut(Keys.D, KeyModifier.Ctrl) },
                {"Frame\\Remove frame", new KeyboardShortcut() },
                {"Frame\\Increase pattern value", new KeyboardShortcut() },
                {"Frame\\Decrease pattern value", new KeyboardShortcut() },

                {"Edit\\Undo", new KeyboardShortcut(Keys.Z, KeyModifier.Ctrl) },
                {"Edit\\Redo", new KeyboardShortcut(Keys.Y, KeyModifier.Ctrl) },
                {"Edit\\Cut", new KeyboardShortcut(Keys.X, KeyModifier.Ctrl) },
                {"Edit\\Copy", new KeyboardShortcut(Keys.C, KeyModifier.Ctrl) },
                {"Edit\\Paste", new KeyboardShortcut(Keys.V, KeyModifier.Ctrl) },
                {"Edit\\Paste and mix", new KeyboardShortcut(Keys.M, KeyModifier.Ctrl) },
                {"Edit\\Backspace", new KeyboardShortcut(Keys.Back) },
                {"Edit\\Insert", new KeyboardShortcut(Keys.Insert) },
                {"Edit\\Delete", new KeyboardShortcut(Keys.Delete) },
                {"Edit\\Select all", new KeyboardShortcut(Keys.A, KeyModifier.Ctrl) },
                {"Edit\\Deselect", new KeyboardShortcut(Keys.Escape) },

                {"Pattern\\Interpolate", new KeyboardShortcut(Keys.G, KeyModifier.Ctrl) },
                {"Pattern\\Reverse", new KeyboardShortcut(Keys.R, KeyModifier.Ctrl) },
                {"Pattern\\Replace instrument", new KeyboardShortcut(Keys.S, KeyModifier.Alt) },
                {"Pattern\\Humanize volumes", new KeyboardShortcut(Keys.H, KeyModifier.Alt) },
                {"Pattern\\Transpose: note down", new KeyboardShortcut(Keys.F1, KeyModifier.Ctrl) },
                {"Pattern\\Transpose: note up", new KeyboardShortcut(Keys.F2, KeyModifier.Ctrl) },
                {"Pattern\\Transpose: octave down", new KeyboardShortcut(Keys.F3, KeyModifier.Ctrl) },
                {"Pattern\\Transpose: octave up", new KeyboardShortcut(Keys.F4, KeyModifier.Ctrl) },
                {"Pattern\\Decrease values", new KeyboardShortcut(Keys.F1, KeyModifier.Shift) },
                {"Pattern\\Increase values", new KeyboardShortcut(Keys.F2, KeyModifier.Shift) },
                {"Pattern\\Coarse decrease values", new KeyboardShortcut(Keys.F3, KeyModifier.Shift) },
                {"Pattern\\Coarse increase values", new KeyboardShortcut(Keys.F4, KeyModifier.Shift) },
                {"Pattern\\Jump to top of frame", new KeyboardShortcut(Keys.Home) },
                {"Pattern\\Jump to bottom of frame", new KeyboardShortcut(Keys.End) },

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
                {"Piano\\3rd octave C", new KeyboardShortcut() },
                {"Piano\\3rd octave C#", new KeyboardShortcut() },
                {"Piano\\3rd octave D", new KeyboardShortcut() },
                {"Piano\\3rd octave D#", new KeyboardShortcut() },
                {"Piano\\3rd octave E", new KeyboardShortcut() },
                {"Piano\\3rd octave F", new KeyboardShortcut() },
                {"Piano\\3rd octave F#", new KeyboardShortcut() },
                {"Piano\\3rd octave G", new KeyboardShortcut() },
                {"Piano\\3rd octave G#", new KeyboardShortcut() },
                {"Piano\\3rd octave A", new KeyboardShortcut() },
                {"Piano\\3rd octave A#", new KeyboardShortcut() },
                {"Piano\\3rd octave B", new KeyboardShortcut() },
                {"Piano\\4th octave C", new KeyboardShortcut() },
                {"Piano\\4th octave C#", new KeyboardShortcut() },
                {"Piano\\4th octave D", new KeyboardShortcut() },
                {"Piano\\4th octave D#", new KeyboardShortcut() },
                {"Piano\\4th octave E", new KeyboardShortcut() },
                {"Piano\\4th octave F", new KeyboardShortcut() },
                {"Piano\\4th octave F#", new KeyboardShortcut() },
                {"Piano\\4th octave G", new KeyboardShortcut() },
                {"Piano\\4th octave G#", new KeyboardShortcut() },
                {"Piano\\4th octave A", new KeyboardShortcut() },
                {"Piano\\4th octave A#", new KeyboardShortcut(Keys.None) },
                {"Piano\\4th octave B", new KeyboardShortcut(Keys.None) },
            };
            public CategoryKeyboard() {
                Initialize();
            }

            public void Validate() {
                int i = 0;
                Dictionary<string, KeyboardShortcut> ret = [];
                foreach (KeyValuePair<string, KeyboardShortcut> pair in defaultShortcuts) {
                    if (!Shortcuts.TryGetValue(pair.Key, out KeyboardShortcut value)) {
                        ret.Add(pair.Key, pair.Value);
                    }
                    else {
                        ret.Add(pair.Key, value);
                    }
                    ++i;
                }
                Shortcuts = ret;
            }

            public void Initialize() {
                Shortcuts = [];
                for (int i = 0; i < defaultShortcuts.Count; i++) {
                    Shortcuts.Add(defaultShortcuts.ElementAt(i).Key, defaultShortcuts.ElementAt(i).Value);
                }
            }
        }

        /// <summary>
        /// If any categories are null, replace them with a new version
        /// </summary>
        private void ValidateCategories() {
            General ??= new();
            Files ??= new();
            PatternEditor ??= new();
            SamplesWaves ??= new();
            MIDI ??= new();
            Audio ??= new();
            Visualizer ??= new();
            Keyboard ??= new();
            Keyboard.Validate();
        }

        /// <summary>
        /// The default settings
        /// </summary>
        public static SettingsProfile Default {
            get {
                return new SettingsProfile();
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
        /// Writes a SettingsProfile to a json formatted file at <c>path\fileName</c>
        /// </summary>
        public static void WriteToDisk(SettingsProfile profileToSave) {
            Directory.CreateDirectory(SaveLoad.SettingsFolderPath);
            using (StreamWriter outputFile = new StreamWriter(SaveLoad.SettingsPath)) {
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() {
                    IncludeFields = true,
                };
                JsonSerializerOptions options = jsonSerializerOptions;
                outputFile.Write(JsonSerializer.Serialize(profileToSave, typeof(SettingsProfile), options));
            }
        }

        /// <summary>
        /// Returns the SettingsProfile read from the given path, if not found, returns a default settings
        /// </summary>
        public static SettingsProfile ReadFromDisk() {
            string jsonString;
            SettingsProfile ret = Default;
            try {
                jsonString = File.ReadAllText(SaveLoad.SettingsPath);
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() {
                    IncludeFields = true,
                };
                JsonSerializerOptions options = jsonSerializerOptions;
                ret = JsonSerializer.Deserialize<SettingsProfile>(jsonString, options);
            } catch {
                WriteToDisk(Default);
            }
            ret.ValidateCategories();

            return ret;
        }
    }
}
