using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            pageSelector = new VerticalListSelector(5, 12, 75, 258, new string[] { "General", "Pattern Editor", "Appearance", "Samples/Waves", "MIDI", "Audio", "Visualizer", "Keyboard" }, this);
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
            pages["General"].AddCheckbox("option1", "");
            pages["General"].AddCheckbox("option2", "");
            pages["General"].AddDropdown("Screen scale:", "", new string[] { "100%", "200%", "300%", "400%", "500%" });
            pages["General"].AddBreak();
            pages["General"].AddLabel("Metering");
            pages["General"].AddDropdown("Oscilloscope mode", "", new string[] { "Mono", "Stereo: split", "Stereo: overlap" });
            pages["General"].AddDropdown("Meter decay rate", "", new string[] { "Slow", "Medium", "Fast" });
            pages["General"].AddDropdown("Meter color mode", "", new string[] { "Flat", "Gradient" });
            pages["General"].AddCheckbox("Flash meter red when clipping", "");

            pages["Files"].AddLabel("Default values");
            pages["Files"].AddNumberBox("Default rows per frame", "", 1, 256);
            pages["Files"].AddTextbox("Default ticks per row", "", 90);


            pages["Pattern Editor"].AddLabel("Pattern Editor");
            pages["Pattern Editor"].AddCheckbox("Show row numbers in hex", "");
            pages["Pattern Editor"].AddCheckbox("Show note off and release as text", "");
            pages["Pattern Editor"].AddCheckbox("Fade volume column", "");
            pages["Pattern Editor"].AddCheckbox("Ignore step when moving", "");
            pages["Pattern Editor"].AddCheckbox("Key repeat", "");
            pages["Pattern Editor"].AddCheckbox("Restore channel state on playback", "");
            pages["Pattern Editor"].AddDropdown("Page jump amount", "", new string[] { "2", "4", "8", "16" });

            pages["Samples/Waves"].AddLabel("Sample Import Settings");
            pages["Samples/Waves"].AddCheckbox("Normalize samples on import", "");
            pages["Samples/Waves"].AddCheckbox("Trim sample silence on import", "");
            pages["Samples/Waves"].AddCheckbox("Resample to 44.1kHz on import", "");
            pages["Samples/Waves"].AddCheckbox("Preview samples in browser", "");
            pages["Samples/Waves"].AddCheckbox("Include samples in visualizer by default", "");
            pages["Samples/Waves"].AddNumberBox("Default base key", "", 12, 131, NumberBox.NumberDisplayMode.Note, -1, 56);
            pages["Samples/Waves"].AddBreak();
            pages["Samples/Waves"].AddLabel("Resampling");
            pages["Samples/Waves"].AddDropdown("Default wave resample mode", "", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });
            pages["Samples/Waves"].AddDropdown("Default sample resample mode", "", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });

            pages["MIDI"].AddLabel("MIDI");
            pages["MIDI"].AddDropdown("Input device", "", new string[] { "(none)" }, false, -1, 999);
            pages["MIDI"].AddCheckbox("Record note velocity", "");

            pages["Audio"].AddLabel("Audio");
            pages["Audio"].AddDropdown("Output device", "", new string[] { "(default)" }, false, -1, 999);
            pages["Audio"].AddNumberBox("Master volume", "", 0, 200);
            pages["Audio"].AddBreak();
            pages["Audio"].AddLabel("Advanced");
            pages["Audio"].AddDropdown("Oversampling", "", new string[] { "Off", "2x (recommended)", "4x", "4x" }, false, -1, 999);

            pages["Visualizer"].AddLabel("Piano");
            pages["Visualizer"].AddNumberBox("Note speed:", "", 1, 20);
            pages["Visualizer"].AddCheckbox("Change note width by volume", "");
            pages["Visualizer"].AddCheckbox("Change note opacity by volume", "");
            pages["Visualizer"].AddCheckbox("Highlight pressed keys", "");
            pages["Visualizer"].AddBreak();
            pages["Visualizer"].AddLabel("Oscilloscopes");
            pages["Visualizer"].AddNumberBox("Wave zoom", "", 50, 200);
            pages["Visualizer"].AddCheckbox("Colorful waves", "");
            pages["Visualizer"].AddDropdown("Wave thickness", "", new string[] { "Thin", "Medium", "Thick" });
            pages["Visualizer"].AddDropdown("Crosshairs", "", new string[] { "None", "Horizontal", "Horizontal + Vertical" });
            pages["Visualizer"].AddCheckbox("Oscilloscope borders", "");

        }

        public new void Open() {
            base.Open();
            MidiInput.ReadMidiDevices();
            (pages["MIDI"]["Input device:"] as ConfigurationOption.Dropdown).SetMenuItems(MidiInput.MidiDevicesNames);
            (pages["Audio"]["Output device:"] as ConfigurationOption.Dropdown).SetMenuItems(Audio.AudioEngine.OutputDeviceNames);
            ReadSettings();
        }

        public void ReadSettings() {
            // (pages["Audio"]["Output device:"] as ConfigurationOption.Dropdown).SetMenuItems(Audio.AudioEngine.dev);
            (pages["Keyboard"] as KeyboardPage).LoadBindingsFrom(App.CurrentSettings.Keyboard.Shortcuts);
        }

        public void ApplySettings() {
            if (!MidiInput.ChangeMidiDevice((pages["MIDI"]["Input device:"] as ConfigurationOption.Dropdown).Value)) {
                pages["MIDI"]["Input device:"].ValueInt = 0;
            }
            (pages["Keyboard"] as KeyboardPage).SaveBindingsInto(App.CurrentSettings.Keyboard.Shortcuts);
        }

        public override void Update() {
            if (windowIsOpen) {
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
            if (windowIsOpen) {
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

            public ConfigurationOption.Dropdown AddDropdown(string label, string description, string[] dropdownItems, bool scrollWrap = false, int dropdownPosition = -1, int dropdownWidth = -1) {
                ypos += 1;
                ConfigurationOption.Dropdown ret = new ConfigurationOption.Dropdown(ypos, label, description, dropdownItems, this, scrollWrap, dropdownPosition, dropdownWidth);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }

            public ConfigurationOption.NumberBox AddNumberBox(string label, string description, int min, int max, NumberBox.NumberDisplayMode displayMode = NumberBox.NumberDisplayMode.Number, int boxPosition = -1, int boxWidth = -1) {
                ypos += 1;
                ConfigurationOption.NumberBox ret = new ConfigurationOption.NumberBox(ypos, label, description, this, boxPosition, boxWidth);
                ret.SetDisplayMode(displayMode);
                ret.SetValueLimits(min, max);
                ypos += ret.height;
                ypos += 1;
                options.Add(label, ret);
                return ret;
            }

            public ConfigurationOption.TextBox AddTextbox(string label, string description, int boxWidth, int boxPosition = -1) {
                ypos += 1;
                ConfigurationOption.TextBox ret = new ConfigurationOption.TextBox(ypos, boxWidth, label, description, this, boxPosition);
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
            public KeyboardPage(Element parent) : base(parent) {
                AddLabel("Keyboard Bindings");
                bindingList = new KeyboardBindingList(4, ypos, width - 8, 16, this);
                bindingList.SetDictionary(App.CurrentSettings.Keyboard.defaultShortcuts);
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

            }

            public override void Update() {
                base.Update();
                bindingList.Update();
            }

            public override void Draw() {
                base.Draw();
                bindingList.Draw();
            }
        }

        class AppearancePage : Page {
            public AppearancePage(Element parent) : base(parent) {
                AddLabel("Appearance");
            }

            public override void Update() {
                base.Update();
            }

            public override void Draw() {
                base.Draw();
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
            public Checkbox(int y, string name, string description, ConfigurationDialog.Page parent) : base(y, 11, name, description, parent) {
                checkbox = new(1, 1, this);
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
                Write(Name, 14, 2, UIColors.labelDark);
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

            public string GetFullString() {
                return
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
            public Dropdown(int y, string name, string description, string[] dropdownItems, ConfigurationDialog.Page parent, bool scrollWrap = true, int dropdownPosition = -1, int dropdownWidth = -1) : base(y, 13, name, description, parent) {
                if (dropdownPosition == -1) {
                    dropdownPosition = Helpers.GetWidthOfText(name) + 8;
                }
                dropdown = new(dropdownPosition, 0, this, scrollWrap);
                if (dropdownWidth != -1) {
                    int minWidth = dropdown.width;
                    int maxWidth = width - dropdownPosition;
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
            public TextBox(int y, int boxWidth, string name, string description, ConfigurationDialog.Page parent, int boxPosition = -1) : base(y, 13, name, description, parent) {
                if (boxPosition == -1) {
                    boxPosition = Helpers.GetWidthOfText(name) + 8;
                }
                textBox = new("", boxPosition, 0, boxWidth, this);
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
            public NumberBox(int y, string name, string description, ConfigurationDialog.Page parent, int numberBoxPosition = -1, int numberBoxWidth = -1) : base(y, 13, name, description, parent) {
                if (numberBoxPosition == -1) {
                    numberBoxPosition = Helpers.GetWidthOfText(name) + 8;
                }
                if (numberBoxWidth != -1) {
                    int minWidth = 38;
                    int maxWidth = width - numberBoxPosition;
                    numberBoxWidth = Math.Clamp(numberBoxWidth, minWidth, maxWidth);
                    numberBox = new("", numberBoxPosition, 0, numberBoxWidth, numberBoxWidth, this);
                }
                else {
                    numberBox = new("", numberBoxPosition, 0, this);
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

    }
    #endregion

}
