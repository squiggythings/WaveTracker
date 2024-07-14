using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WaveTracker.UI {
    public class ConfigurationDialog : Dialog {
        public Button ok, cancel, apply;
        public Button closeButton;
        VerticalListSelector pageSelector;
        public Dictionary<string, Page> pages;

        public ConfigurationDialog() : base("Preferences...", 376, 290) {
            apply = AddNewBottomButton("Apply", this);
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            pages = new Dictionary<string, Page>();
            pageSelector = new VerticalListSelector(5, 12, 75, 258, new string[] { "General", "Files", "Pattern Editor", "Appearance", "Samples/Waves", "MIDI", "Audio", "Visualizer", "Keyboard" }, this);
            pages.Add("General", new Page(this));
            pages.Add("Files", new Page(this));
            pages.Add("Appearance", new AppearancePage(this));
            pages.Add("Pattern Editor", new Page(this));
            pages.Add("Samples/Waves", new Page(this));
            pages.Add("MIDI", new Page(this));
            pages.Add("Audio", new Page(this));
            pages.Add("Visualizer", new Page(this));
            pages.Add("Keyboard", new KeyboardPage(this));

            pages["General"].AddLabel("General");
            pages["General"].AddDropdown("Screen scale", "", new string[] { "100%", "200%", "300%", "400%", "500%" });
            pages["General"].AddBreak();
            pages["General"].AddLabel("Metering");
            pages["General"].AddDropdown("Oscilloscope mode", "", new string[] { "Mono", "Stereo: split", "Stereo: overlap" }, width: 88);
            pages["General"].AddDropdown("Meter decay rate", "", new string[] { "Slow", "Medium", "Fast" }, width: 88);
            pages["General"].AddDropdown("Meter color mode", "", new string[] { "Flat", "Gradient" }, positionOffset: 1, width: 88);
            pages["General"].AddCheckbox("Flash meter red when clipping", "");

            pages["Files"].AddLabel("Default module info");
            pages["Files"].AddTextbox("Default ticks per row", "", width: 38, positionOffset: 52);
            pages["Files"].AddNumberBox("Default rows per frame", "", 1, 256, positionOffset: 34);
            pages["Files"].AddNumberBox("Default number of channels", "", 1, 24, positionOffset: 17);
            pages["Files"].AddTextbox("Default author name", "", 110, positionOffset: 55);
            pages["Files"].AddNumberBox("Default row highlight primary", "", 1, 256, positionOffset: 12);
            pages["Files"].AddNumberBox("Default row highlight secondary", "", 1, 256);

            pages["Pattern Editor"].AddLabel("Pattern Editor");
            pages["Pattern Editor"].AddCheckbox("Show row numbers in hex", "");
            pages["Pattern Editor"].AddCheckbox("Show note off/release as text", "");
            pages["Pattern Editor"].AddCheckbox("Fade volume column", "");
            pages["Pattern Editor"].AddCheckbox("Show previous/next patterns", "");
            pages["Pattern Editor"].AddCheckbox("Ignore step when moving", "");
            pages["Pattern Editor"].AddDropdown("Step after numeric input", "", new string[] { "Always", "At the end of a cell", "After cell, including effect", "Never" });
            pages["Pattern Editor"].AddCheckbox("Preview notes on input", "");
            pages["Pattern Editor"].AddCheckbox("Wrap cursor horizontally", "");
            pages["Pattern Editor"].AddCheckbox("Wrap cursor across frames", "");
            pages["Pattern Editor"].AddCheckbox("Key repeat", "");
            pages["Pattern Editor"].AddDropdown("Page jump amount", "", new string[] { "1", "2", "4", "8", "16" });
            pages["Pattern Editor"].AddCheckbox("Restore channel state on playback", "");

            pages["Samples/Waves"].AddLabel("Sample Import Settings");
            pages["Samples/Waves"].AddCheckbox("Automatically normalize samples on import", "");
            pages["Samples/Waves"].AddCheckbox("Automatically trim sample silence on import", "");
            pages["Samples/Waves"].AddCheckbox("Automatically preview samples in browser", "");
            pages["Samples/Waves"].AddCheckbox("Include samples in visualizer by default", "");
            pages["Samples/Waves"].AddNumberBox("Default base key", "", 12, 131, NumberBox.NumberDisplayMode.Note, 60);
            pages["Samples/Waves"].AddBreak();
            pages["Samples/Waves"].AddLabel("Resampling");
            pages["Samples/Waves"].AddDropdown("Default wave resample mode", "", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" }, positionOffset: 7);
            pages["Samples/Waves"].AddDropdown("Default sample resample mode", "", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });

            pages["MIDI"].AddLabel("MIDI");
            pages["MIDI"].AddDropdown("Input device", "", new string[] { "(none)" }, false, -1, 999);
            pages["MIDI"].AddBreak();
            pages["MIDI"].AddLabel("Transpose");
            pages["MIDI"].AddNumberBox("MIDI transpose", "", -48, 48, NumberBox.NumberDisplayMode.PlusMinus);
            pages["MIDI"].AddCheckbox("Apply octave transpose", "");
            pages["MIDI"].AddBreak();
            pages["MIDI"].AddLabel("MIDI messages");
            pages["MIDI"].AddCheckbox("Record note velocity", "");
            pages["MIDI"].AddCheckbox("Use program change to select instrument", "");
            pages["MIDI"].AddCheckbox("Receive play/stop messages", "");
            pages["MIDI"].AddCheckbox("Receive record messages", "");


            pages["Audio"].AddLabel("Audio");
            pages["Audio"].AddDropdown("Output device", "", new string[] { "(default)" }, false, -1, 999);
            pages["Audio"].AddBreak();
            pages["Audio"].AddSlider("Volume", "", 32, 200, 10, 0, 200, "%");
            pages["Audio"].AddBreak();
            pages["Audio"].AddDropdown("Sample rate", "", new string[] { "11025 Hz", "22050 Hz", "44100 Hz", "48000 Hz", "96000 Hz" }, false, positionOffset: 8);
            pages["Audio"].AddBreak();
            pages["Audio"].AddLabel("Advanced");
            pages["Audio"].AddDropdown("Oversampling", "", new string[] { "1x", "2x (recommended)", "4x", "8x" }, false);

            pages["Visualizer"].AddLabel("Piano");
            pages["Visualizer"].AddSlider("Note speed", "", 18, 95, 0, 1, 20);
            pages["Visualizer"].AddCheckbox("Change note width by volume", "");
            pages["Visualizer"].AddCheckbox("Change note opacity by volume", "");
            pages["Visualizer"].AddCheckbox("Highlight pressed keys", "");
            pages["Visualizer"].AddBreak();
            pages["Visualizer"].AddLabel("Oscilloscopes");
            pages["Visualizer"].AddNumberBox("Wave zoom", "", 50, 200, NumberBox.NumberDisplayMode.Percent);
            pages["Visualizer"].AddCheckbox("Colorful waves", "");
            pages["Visualizer"].AddDropdown("Wave thickness", "", new string[] { "Thin", "Medium", "Thick" });
            pages["Visualizer"].AddDropdown("Crosshairs", "", new string[] { "None", "Horizontal", "Horizontal + Vertical" });
            pages["Visualizer"].AddCheckbox("Oscilloscope borders", "");

        }

        public new void Open() {
            base.Open();
            MidiInput.ReadMidiDevices();
            (pages["MIDI"]["Input device"] as ConfigurationOption.Dropdown).SetMenuItems(MidiInput.MidiDevicesNames);

            string[] audioDeviceOptions = new string[Audio.AudioEngine.OutputDevices.Count + 1];
            audioDeviceOptions[0] = "(default)";
            for (int i = 0; i < Audio.AudioEngine.OutputDevices.Count; i++) {
                audioDeviceOptions[i + 1] = Audio.AudioEngine.OutputDeviceNames[i];
            }
            (pages["Audio"]["Output device"] as ConfigurationOption.Dropdown).SetMenuItems(audioDeviceOptions);

            ReadSettings();
        }

        /// <summary>
        /// Reads the app's settings profile into the controls on this dialog
        /// </summary>
        void ReadSettings() {
            // (pages["Audio"]["Output device:"] as ConfigurationOption.Dropdown).SetMenuItems(Audio.AudioEngine.dev);
            pages["General"]["Screen scale"].ValueInt = App.Settings.General.ScreenScale - 1;
            pages["General"]["Oscilloscope mode"].ValueInt = App.Settings.General.OscilloscopeMode;
            pages["General"]["Meter decay rate"].ValueInt = App.Settings.General.MeterDecayRate;
            pages["General"]["Meter color mode"].ValueInt = App.Settings.General.MeterColorMode;
            pages["General"]["Flash meter red when clipping"].ValueBool = App.Settings.General.FlashMeterRedWhenClipping;
            pages["Files"]["Default ticks per row"].ValueString = App.Settings.Files.DefaultTicksPerRow;
            pages["Files"]["Default rows per frame"].ValueInt = App.Settings.Files.DefaultRowsPerFrame;
            pages["Files"]["Default number of channels"].ValueInt = App.Settings.Files.DefaultNumberOfChannels;
            pages["Files"]["Default author name"].ValueString = App.Settings.Files.DefaultAuthorName;
            pages["Files"]["Default row highlight primary"].ValueInt = App.Settings.Files.DefaultRowPrimaryHighlight;
            pages["Files"]["Default row highlight secondary"].ValueInt = App.Settings.Files.DefaultRowSecondaryHighlight;

            (pages["Appearance"] as AppearancePage).LoadColorsFrom(App.Settings.Appearance.Theme);

            pages["Pattern Editor"]["Show row numbers in hex"].ValueBool = App.Settings.PatternEditor.ShowRowNumbersInHex;
            pages["Pattern Editor"]["Show note off/release as text"].ValueBool = App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText;
            pages["Pattern Editor"]["Fade volume column"].ValueBool = App.Settings.PatternEditor.FadeVolumeColumn;
            pages["Pattern Editor"]["Show previous/next patterns"].ValueBool = App.Settings.PatternEditor.ShowPreviousNextPatterns;
            pages["Pattern Editor"]["Preview notes on input"].ValueBool = App.Settings.PatternEditor.PreviewNotesOnInput;
            pages["Pattern Editor"]["Ignore step when moving"].ValueBool = App.Settings.PatternEditor.IgnoreStepWhenMoving;
            pages["Pattern Editor"]["Step after numeric input"].ValueInt = (int)App.Settings.PatternEditor.StepAfterNumericInput;
            pages["Pattern Editor"]["Preview notes on input"].ValueBool = App.Settings.PatternEditor.PreviewNotesOnInput;
            pages["Pattern Editor"]["Wrap cursor horizontally"].ValueBool = App.Settings.PatternEditor.WrapCursorHorizontally;
            pages["Pattern Editor"]["Wrap cursor across frames"].ValueBool = App.Settings.PatternEditor.WrapCursorAcrossFrames;
            pages["Pattern Editor"]["Key repeat"].ValueBool = App.Settings.PatternEditor.KeyRepeat;
            pages["Pattern Editor"]["Page jump amount"].ValueInt = (int)Math.Log2(App.Settings.PatternEditor.PageJumpAmount);
            pages["Pattern Editor"]["Restore channel state on playback"].ValueBool = App.Settings.PatternEditor.RestoreChannelState;


            pages["Samples/Waves"]["Automatically normalize samples on import"].ValueBool = App.Settings.SamplesWaves.AutomaticallyNormalizeSamplesOnImport;
            pages["Samples/Waves"]["Automatically trim sample silence on import"].ValueBool = App.Settings.SamplesWaves.AutomaticallyTrimSamples;
            pages["Samples/Waves"]["Include samples in visualizer by default"].ValueBool = App.Settings.SamplesWaves.IncludeSamplesInVisualizerByDefault;
            pages["Samples/Waves"]["Default base key"].ValueInt = App.Settings.SamplesWaves.DefaultSampleBaseKey;
            pages["Samples/Waves"]["Default wave resample mode"].ValueInt = (int)App.Settings.SamplesWaves.DefaultResampleModeWave;
            pages["Samples/Waves"]["Default sample resample mode"].ValueInt = (int)App.Settings.SamplesWaves.DefaultResampleModeSample;

            pages["MIDI"]["Input device"].ValueInt = 0; // reset to none incase the one from settings is not found
            pages["MIDI"]["Input device"].ValueString = App.Settings.MIDI.InputDevice;
            pages["MIDI"]["MIDI transpose"].ValueInt = App.Settings.MIDI.MIDITranspose;
            pages["MIDI"]["Apply octave transpose"].ValueBool = App.Settings.MIDI.ApplyOctaveTranspose;
            pages["MIDI"]["Record note velocity"].ValueBool = App.Settings.MIDI.RecordNoteVelocity;
            pages["MIDI"]["Use program change to select instrument"].ValueBool = App.Settings.MIDI.UseProgramChangeToSelectInstrument;
            pages["MIDI"]["Receive play/stop messages"].ValueBool = App.Settings.MIDI.ReceivePlayStopMessages;
            pages["MIDI"]["Receive record messages"].ValueBool = App.Settings.MIDI.ReceiveRecordMessages;
            pages["MIDI"]["Receive record messages"].ValueBool = App.Settings.MIDI.ReceiveRecordMessages;

            pages["Audio"]["Output device"].ValueString = App.Settings.Audio.OutputDevice;
            pages["Audio"]["Volume"].ValueInt = App.Settings.Audio.MasterVolume;
            pages["Audio"]["Sample rate"].ValueInt = (int)App.Settings.Audio.SampleRate;
            pages["Audio"]["Oversampling"].ValueInt = (int)Math.Log2(App.Settings.Audio.Oversampling);

            pages["Visualizer"]["Note speed"].ValueInt = App.Settings.Visualizer.PianoSpeed;
            pages["Visualizer"]["Change note width by volume"].ValueBool = App.Settings.Visualizer.ChangeNoteWidthByVolume;
            pages["Visualizer"]["Change note opacity by volume"].ValueBool = App.Settings.Visualizer.ChangeNoteOpacityByVolume;
            pages["Visualizer"]["Highlight pressed keys"].ValueBool = App.Settings.Visualizer.HighlightPressedKeys;
            pages["Visualizer"]["Wave zoom"].ValueInt = App.Settings.Visualizer.OscilloscopeZoom;
            pages["Visualizer"]["Colorful waves"].ValueBool = App.Settings.Visualizer.OscilloscopeColorfulWaves;
            pages["Visualizer"]["Wave thickness"].ValueInt = App.Settings.Visualizer.OscilloscopeThickness;
            pages["Visualizer"]["Crosshairs"].ValueInt = App.Settings.Visualizer.OscilloscopeCrosshairs;
            pages["Visualizer"]["Oscilloscope borders"].ValueBool = App.Settings.Visualizer.OscilloscopeBorders;

            (pages["Keyboard"] as KeyboardPage).LoadBindingsFrom(App.Settings.Keyboard.Shortcuts);

        }
        /// <summary>
        /// Reads the controls on this dialog into the app's settings profile
        /// </summary>
        void ApplySettings() {


            App.Settings.General.ScreenScale = pages["General"]["Screen scale"].ValueInt + 1;
            while (App.ClientWindow.ClientBounds.Height / App.Settings.General.ScreenScale < height) {
                App.Settings.General.ScreenScale--;
            }
            pages["General"]["Screen scale"].ValueInt = App.Settings.General.ScreenScale - 1;

            App.Settings.General.OscilloscopeMode = pages["General"]["Oscilloscope mode"].ValueInt;
            App.Settings.General.MeterDecayRate = pages["General"]["Meter decay rate"].ValueInt;
            App.Settings.General.MeterColorMode = pages["General"]["Meter color mode"].ValueInt;
            App.Settings.General.FlashMeterRedWhenClipping = pages["General"]["Flash meter red when clipping"].ValueBool;
            App.Settings.Files.DefaultTicksPerRow = pages["Files"]["Default ticks per row"].ValueString;
            App.Settings.Files.DefaultRowsPerFrame = pages["Files"]["Default rows per frame"].ValueInt;
            App.Settings.Files.DefaultNumberOfChannels = pages["Files"]["Default number of channels"].ValueInt;
            App.Settings.Files.DefaultAuthorName = pages["Files"]["Default author name"].ValueString;
            App.Settings.Files.DefaultRowPrimaryHighlight = pages["Files"]["Default row highlight primary"].ValueInt;
            App.Settings.Files.DefaultRowSecondaryHighlight = pages["Files"]["Default row highlight secondary"].ValueInt;

            (pages["Appearance"] as AppearancePage).SaveColorsInto(App.Settings.Appearance.Theme);

            App.Settings.PatternEditor.ShowRowNumbersInHex = pages["Pattern Editor"]["Show row numbers in hex"].ValueBool;
            App.Settings.PatternEditor.ShowNoteOffAndReleaseAsText = pages["Pattern Editor"]["Show note off/release as text"].ValueBool;
            App.Settings.PatternEditor.FadeVolumeColumn = pages["Pattern Editor"]["Fade volume column"].ValueBool;
            App.Settings.PatternEditor.ShowPreviousNextPatterns = pages["Pattern Editor"]["Show previous/next patterns"].ValueBool;
            App.Settings.PatternEditor.PreviewNotesOnInput = pages["Pattern Editor"]["Preview notes on input"].ValueBool;
            App.Settings.PatternEditor.IgnoreStepWhenMoving = pages["Pattern Editor"]["Ignore step when moving"].ValueBool;
            App.Settings.PatternEditor.StepAfterNumericInput = (SettingsProfile.MoveToNextRowBehavior)pages["Pattern Editor"]["Step after numeric input"].ValueInt;
            App.Settings.PatternEditor.PreviewNotesOnInput = pages["Pattern Editor"]["Preview notes on input"].ValueBool;
            App.Settings.PatternEditor.WrapCursorHorizontally = pages["Pattern Editor"]["Wrap cursor horizontally"].ValueBool;
            App.Settings.PatternEditor.WrapCursorAcrossFrames = pages["Pattern Editor"]["Wrap cursor across frames"].ValueBool;
            App.Settings.PatternEditor.KeyRepeat = pages["Pattern Editor"]["Key repeat"].ValueBool;
            App.Settings.PatternEditor.PageJumpAmount = (int)Math.Pow(2, pages["Pattern Editor"]["Page jump amount"].ValueInt);
            App.Settings.PatternEditor.RestoreChannelState = pages["Pattern Editor"]["Restore channel state on playback"].ValueBool;


            App.Settings.SamplesWaves.AutomaticallyNormalizeSamplesOnImport = pages["Samples/Waves"]["Automatically normalize samples on import"].ValueBool;
            App.Settings.SamplesWaves.AutomaticallyTrimSamples = pages["Samples/Waves"]["Automatically trim sample silence on import"].ValueBool;
            App.Settings.SamplesWaves.IncludeSamplesInVisualizerByDefault = pages["Samples/Waves"]["Include samples in visualizer by default"].ValueBool;
            App.Settings.SamplesWaves.DefaultSampleBaseKey = pages["Samples/Waves"]["Default base key"].ValueInt;
            App.Settings.SamplesWaves.DefaultResampleModeWave = (Audio.ResamplingMode)pages["Samples/Waves"]["Default wave resample mode"].ValueInt;
            App.Settings.SamplesWaves.DefaultResampleModeSample = (Audio.ResamplingMode)pages["Samples/Waves"]["Default sample resample mode"].ValueInt;

            if (!MidiInput.ChangeMidiDevice((pages["MIDI"]["Input device"] as ConfigurationOption.Dropdown).Value)) {
                pages["MIDI"]["Input device"].ValueInt = 0;
            }
            App.Settings.MIDI.InputDevice = pages["MIDI"]["Input device"].ValueString;
            App.Settings.MIDI.MIDITranspose = pages["MIDI"]["MIDI transpose"].ValueInt;
            App.Settings.MIDI.ApplyOctaveTranspose = pages["MIDI"]["Apply octave transpose"].ValueBool;
            App.Settings.MIDI.RecordNoteVelocity = pages["MIDI"]["Record note velocity"].ValueBool;
            App.Settings.MIDI.UseProgramChangeToSelectInstrument = pages["MIDI"]["Use program change to select instrument"].ValueBool;
            App.Settings.MIDI.ReceivePlayStopMessages = pages["MIDI"]["Receive play/stop messages"].ValueBool;
            App.Settings.MIDI.ReceiveRecordMessages = pages["MIDI"]["Receive record messages"].ValueBool;
            App.Settings.MIDI.ReceiveRecordMessages = pages["MIDI"]["Receive record messages"].ValueBool;

            App.Settings.Audio.MasterVolume = pages["Audio"]["Volume"].ValueInt;
            if (App.Settings.Audio.OutputDevice != pages["Audio"]["Output device"].ValueString ||
                App.Settings.Audio.SampleRate != (Audio.SampleRate)pages["Audio"]["Sample rate"].ValueInt ||
                App.Settings.Audio.Oversampling != (int)Math.Pow(2, pages["Audio"]["Oversampling"].ValueInt)) {

                App.Settings.Audio.OutputDevice = pages["Audio"]["Output device"].ValueString;
                App.Settings.Audio.SampleRate = (Audio.SampleRate)pages["Audio"]["Sample rate"].ValueInt;
                App.Settings.Audio.Oversampling = (int)Math.Pow(2, pages["Audio"]["Oversampling"].ValueInt);
                Audio.AudioEngine.Reset();
            }


            App.Settings.Visualizer.PianoSpeed = pages["Visualizer"]["Note speed"].ValueInt;
            App.Settings.Visualizer.ChangeNoteWidthByVolume = pages["Visualizer"]["Change note width by volume"].ValueBool;
            App.Settings.Visualizer.ChangeNoteOpacityByVolume = pages["Visualizer"]["Change note opacity by volume"].ValueBool;
            App.Settings.Visualizer.HighlightPressedKeys = pages["Visualizer"]["Highlight pressed keys"].ValueBool;
            App.Settings.Visualizer.OscilloscopeZoom = pages["Visualizer"]["Wave zoom"].ValueInt;
            App.Settings.Visualizer.OscilloscopeColorfulWaves = pages["Visualizer"]["Colorful waves"].ValueBool;
            App.Settings.Visualizer.OscilloscopeThickness = pages["Visualizer"]["Wave thickness"].ValueInt;
            App.Settings.Visualizer.OscilloscopeCrosshairs = pages["Visualizer"]["Crosshairs"].ValueInt;
            App.Settings.Visualizer.OscilloscopeBorders = pages["Visualizer"]["Oscilloscope borders"].ValueBool;

            (pages["Keyboard"] as KeyboardPage).SaveBindingsInto(App.Settings.Keyboard.Shortcuts);

            SettingsProfile.WriteToDisk(App.Settings);
        }

        public override void Update() {
            if (WindowIsOpen) {
                DoDragging();
                if (ExitButton.Clicked || cancel.Clicked) {
                    Close();
                }
                if (ok.Clicked) {
                    ApplySettings();
                    if (Input.focus == this)
                        Close();
                }
                if (apply.Clicked) {
                    ApplySettings();
                }
                pageSelector.Update();
                if (pages.TryGetValue(pageSelector.SelectedItem, out Page p)) {
                    p.Update();
                }
            }
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                pageSelector.Draw();
                if (pages.TryGetValue(pageSelector.SelectedItem, out Page p)) {
                    p.Draw();
                }
            }
        }


        #region Page definitions
        public class Page : Clickable {
            protected Dictionary<string, ConfigurationOption> options;
            protected int ypos = 2;
            public Page(Element parent) {
                x = 82;
                y = 12;
                width = 289;
                height = 258;
                options = new Dictionary<string, ConfigurationOption>();
                SetParent(parent);
            }

            public ConfigurationOption.Checkbox AddCheckbox(string label, string description) {
                ypos += 1;
                ConfigurationOption.Checkbox ret = new ConfigurationOption.Checkbox(ypos, label, description, this);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }
            public ConfigurationOption.Label AddLabel(string label) {
                ypos += 1;
                ConfigurationOption.Label ret = new ConfigurationOption.Label(ypos, label, this);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }

            public ConfigurationOption this[string name] {
                get {
                    return options[name];
                }
            }

            public void AddBreak(int height = 5) {
                ypos += height;
            }

            public ConfigurationOption.Dropdown AddDropdown(string label, string description, string[] dropdownItems, bool scrollWrap = false, int positionOffset = 0, int width = -0) {
                ypos += 1;
                ConfigurationOption.Dropdown ret = new ConfigurationOption.Dropdown(ypos, label, description, dropdownItems, this, scrollWrap, positionOffset, width);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }

            public ConfigurationOption.NumberBox AddNumberBox(string label, string description, int min, int max, NumberBox.NumberDisplayMode displayMode = NumberBox.NumberDisplayMode.Number, int boxWidth = -1, int positionOffset = 0) {
                ypos += 1;
                ConfigurationOption.NumberBox ret = new ConfigurationOption.NumberBox(ypos, label, description, this, positionOffset, boxWidth);
                ret.SetDisplayMode(displayMode);
                ret.SetValueLimits(min, max);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }

            public ConfigurationOption.TextBox AddTextbox(string label, string description, int width, int positionOffset = 0) {
                ypos += 1;
                ConfigurationOption.TextBox ret = new ConfigurationOption.TextBox(ypos, width, label, description, this, positionOffset);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }
            public ConfigurationOption.Slider AddSlider(string label, string description, int sliderPositionOffset, int width, int numTicks, int min, int max, string suffix = "") {
                ypos += 1;
                ConfigurationOption.Slider ret = new ConfigurationOption.Slider(ypos, label, description, this, sliderPositionOffset, width, numTicks, min, max, suffix);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }

            public virtual void Update() {
                foreach (ConfigurationOption option in options.Values) {
                    option.Update();
                }
            }

            public virtual void Draw() {
                DrawRoundedRect(0, 0, width, height, Color.White);
                foreach (ConfigurationOption option in options.Values) {
                    option.Draw();
                }
            }
        }

        class KeyboardPage : Page {
            KeyboardBindingList bindingList;
            Button resetAll;
            public KeyboardPage(Element parent) : base(parent) {
                AddLabel("Keyboard Bindings");
                bindingList = new KeyboardBindingList(4, ypos, width - 8, 16, this);
                bindingList.SetDictionary(SettingsProfile.CategoryKeyboard.defaultShortcuts);
                resetAll = new Button("Reset All", 0, bindingList.y + bindingList.height + 4, this);
                resetAll.x = bindingList.x + bindingList.width - resetAll.width;
            }

            /// <summary>
            /// Sets keyboard binding ui's bindings from the given dictionary
            /// </summary>
            /// <param name="bindings"></param>
            public void LoadBindingsFrom(Dictionary<string, KeyboardShortcut> bindings) {
                bindingList.SetDictionary(bindings);
            }

            /// <summary>
            /// Saves the keyboard binding ui's bindings into the given dictionary
            /// </summary>
            /// <param name="bindings"></param>
            public void SaveBindingsInto(Dictionary<string, KeyboardShortcut> bindings) {
                bindingList.SaveDictionaryInto(bindings);
            }

            public override void Update() {
                base.Update();
                bindingList.Update();
                if (resetAll.Clicked) {
                    bindingList.ResetAll();
                }
            }

            public override void Draw() {
                base.Draw();
                bindingList.Draw();
                resetAll.Draw();
            }
        }

        class AppearancePage : Page {
            ColorButtonList colorList;
            Button openThemeFiles;
            Button loadTheme;
            Button saveTheme;
            public AppearancePage(Element parent) : base(parent) {
                AddLabel("Appearance");
                colorList = new ColorButtonList(4, ypos, width - 8, 9, this);
                saveTheme = new Button("Save As...", 4, colorList.y + colorList.height + 4, 55, this);
                loadTheme = new Button("Load...", saveTheme.x + saveTheme.width + 4, colorList.y + colorList.height + 4, 55, this);
                openThemeFiles = new Button("Open themes folder...", 0, colorList.y + colorList.height + 4, this);
                openThemeFiles.x = colorList.x + colorList.width - openThemeFiles.width;
            }

            public override void Update() {
                base.Update();
                if (openThemeFiles.Clicked) {
                    System.Diagnostics.Process.Start("explorer.exe", SaveLoad.ThemeFolderPath);
                }
                if (loadTheme.Clicked) {
                    if (SaveLoad.GetThemePathThroughOpenDialog(out string filepath)) {
                        LoadColorsFrom(ColorTheme.FromString(File.ReadAllText(filepath)));
                    }
                    colorList.ResetView();
                }
                if (saveTheme.Clicked) {
                    if (SaveLoad.ChooseThemeSaveLocation(out string filepath)) {
                        ColorTheme theme = new ColorTheme();
                        SaveColorsInto(theme);
                        File.WriteAllText(filepath, ColorTheme.CreateString(theme));
                    }
                }
                colorList.Update();
            }

            public override void Draw() {
                base.Draw();
                colorList.Draw();
                openThemeFiles.Draw();
                loadTheme.Draw();
                saveTheme.Draw();
            }

            public void LoadColorsFrom(ColorTheme theme) {
                colorList.SetDictionary(theme.Colors);
            }
            public void SaveColorsInto(ColorTheme theme) {
                colorList.SaveDictionaryInto(theme.Colors);
            }
        }
        #endregion
    }

    #region Option definitions
    public class ConfigurationOption : Clickable {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public ConfigurationOption(int y, int height, string name, string description, ConfigurationDialog.Page parent) {
            Name = name;
            Description = description;
            this.height = height;
            width = parent.width - 8;
            x = 4;
            this.y = y;
            this.height = height;
            SetParent(parent);
        }

        public virtual void Update() { }

        public virtual void Draw() { }

        public virtual string GetValue() {
            throw new NotImplementedException();
        }
        public virtual void SetValue(string s) {

        }

        public virtual int ValueInt {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public virtual bool ValueBool {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public virtual string ValueString {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public virtual Color ValueColor {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public class Label : ConfigurationOption {
            const int textPadding = 3;
            const int textLocation = 9;
            int textWidth;
            public Label(int y, string name, ConfigurationDialog.Page parent) : base(y, 9, name, "", parent) {
                textWidth = Helpers.GetWidthOfText(name);
            }

            public override void Draw() {
                Write(Name, textLocation + 1, 1, UIColors.labelLight);
                DrawRect(0, 4, textLocation - textPadding, 1, UIColors.labelLight);
                DrawRect(textLocation + textPadding + textWidth + textPadding, 4, width - textWidth - textLocation - textPadding * 2, 1, UIColors.labelLight);
            }
        }

        public class Checkbox : ConfigurationOption {
            UI.Checkbox checkbox;
            public bool Value {
                get {
                    return checkbox.Value;
                }
                set {
                    checkbox.Value = value;
                }
            }
            public bool ValueWasChangedInternally { get; private set; }
            public Checkbox(int y, string name, string description, ConfigurationDialog.Page parent) : base(y, 13, name, description, parent) {
                checkbox = new(1, 2, this);
            }
            public override void Update() {
                if (Clicked) {
                    if (Clicked) {
                        Value = !Value;
                        ValueWasChangedInternally = true;
                    }
                    else {
                        ValueWasChangedInternally = false;
                    }
                }
            }
            public override void Draw() {
                if (IsHovered) {
                    DrawRect(0, 0, width, height, new Color(219, 237, 255));
                }
                checkbox.Draw(IsHovered, IsPressed);
                Write(Name, 14, 3, UIColors.labelDark);
            }

            public override bool ValueBool {
                get { return Value; }
                set { Value = value; }
            }

            public override string GetValue() {
                return Value + "";
            }
            public override void SetValue(string s) {
                bool b = Value;
                if (bool.TryParse(s, out b)) {
                    Value = b;
                }
            }


        }
        public class Dropdown : ConfigurationOption {
            UI.Dropdown dropdown;

            public int Value {
                get {
                    return dropdown.Value;
                }
                set {
                    dropdown.Value = value;
                }
            }
            public bool ValueWasChangedInternally => dropdown.ValueWasChangedInternally;
            public Dropdown(int y, string name, string description, string[] dropdownItems, ConfigurationDialog.Page parent, bool scrollWrap = true, int dropdownPositionOffset = 0, int dropdownWidth = -1) : base(y, 13, name, description, parent) {

                dropdown = new(Helpers.GetWidthOfText(name) + dropdownPositionOffset + 8, 0, this, scrollWrap);
                if (dropdownWidth != -1) {
                    int minWidth = dropdown.width;
                    int maxWidth = width - (Helpers.GetWidthOfText(name) + dropdownPositionOffset + 8);
                    dropdown.width = Math.Clamp(dropdownWidth, minWidth, maxWidth);
                }

                dropdown.SetMenuItems(dropdownItems);
            }

            public void SetMenuItems(string[] items) {
                dropdown.SetMenuItems(items);
            }

            public override void Update() {
                dropdown.Update();
            }
            public override void Draw() {
                Write(Name + ":", 0, 3, UIColors.labelDark);
                dropdown.Draw();
            }

            public override int ValueInt {
                get { return Value; }
                set { Value = value; }
            }

            public override string ValueString {
                get { return dropdown.ValueName; }
                set { dropdown.ValueName = value; }
            }
        }

        public class TextBox : ConfigurationOption {
            Textbox textBox;

            public string Text {
                get {
                    return textBox.Text;
                }
                set {
                    textBox.Text = value;
                }
            }
            public bool ValueWasChangedInternally => textBox.ValueWasChangedInternally;
            public TextBox(int y, int boxWidth, string name, string description, ConfigurationDialog.Page parent, int boxPositionOffset = 0) : base(y, 13, name, description, parent) {
                textBox = new("", Helpers.GetWidthOfText(name) + 8 + boxPositionOffset, 0, boxWidth, this);
            }

            public override void Update() {
                textBox.Update();
            }
            public override void Draw() {
                Write(Name + ":", 0, 3, UIColors.labelDark);
                textBox.Draw();
            }

            public override string ValueString {
                get { return Text; }
                set { Text = value; }
            }
        }

        public class NumberBox : ConfigurationOption {
            UI.NumberBox numberBox;

            public int Value => numberBox.Value;
            public bool ValueWasChangedInternally => numberBox.ValueWasChangedInternally;
            public NumberBox(int y, string name, string description, ConfigurationDialog.Page parent, int numberBoxPositionOffset = 0, int numberBoxWidth = -1) : base(y, 13, name, description, parent) {

                if (numberBoxWidth != -1) {
                    int minWidth = 38;
                    int maxWidth = width - numberBoxPositionOffset;
                    numberBoxWidth = Math.Clamp(numberBoxWidth, minWidth, maxWidth);
                    numberBox = new("", Helpers.GetWidthOfText(name) + 8 + numberBoxPositionOffset, 0, numberBoxWidth, numberBoxWidth, this);
                }
                else {
                    numberBox = new("", Helpers.GetWidthOfText(name) + 8 + numberBoxPositionOffset, 0, this);
                }

            }

            public void SetValueLimits(int min, int max) {
                numberBox.SetValueLimits(min, max);
            }

            public void SetDisplayMode(UI.NumberBox.NumberDisplayMode displayMode) {
                numberBox.DisplayMode = displayMode;
            }

            public override void Update() {
                numberBox.Update();
            }
            public override void Draw() {
                Write(Name + ":", 0, 3, UIColors.labelDark);
                numberBox.Draw();
            }
            public override int ValueInt {
                get { return Value; }
                set { numberBox.Value = value; }
            }
        }

        public class Slider : ConfigurationOption {
            HorizontalSlider slider;
            string suffix;
            public int Value => slider.Value;
            public Slider(int y, string name, string description, ConfigurationDialog.Page parent, int sliderPositionOffset, int width, int numTicks, int min, int max, string suffix) : base(y, 13, name, description, parent) {
                slider = new HorizontalSlider(Helpers.GetWidthOfText(name) + sliderPositionOffset + 8, 0, width, numTicks, this);
                this.suffix = suffix;
                slider.SetValueLimits(min, max);
            }

            public override void Update() {
                slider.Update();
            }
            public override void Draw() {
                Write(Name + ": " + Value + "" + suffix, 0, 3, UIColors.labelDark);
                slider.Draw();
            }
            public override int ValueInt {
                get { return Value; }
                set { slider.Value = value; }
            }
        }

    }
    #endregion

}
