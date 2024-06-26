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
            pages.Add("Pattern Editor", new Page(this));
            pages.Add("Samples/Waves", new Page(this));
            pages.Add("MIDI", new Page(this));

            pages["General"].AddLabel("General");
            pages["General"].AddCheckbox("option1", "");
            pages["General"].AddCheckbox("option2", "");
            pages["General"].AddDropdown("Screen scale:", "", new string[] { "100%", "200%", "300%", "400%", "500%" }, false);
            pages["General"].AddBreak();
            pages["General"].AddLabel("Metering");
            pages["General"].AddDropdown("Oscilloscope mode:", "", new string[] { "Mono", "Stereo: split", "Stereo: overlap" });
            pages["General"].AddDropdown("Meter decay rate:", "", new string[] { "Slow", "Medium", "Fast" });
            pages["General"].AddDropdown("Meter color mode:", "", new string[] { "Flat", "Gradient" });
            pages["General"].AddCheckbox("Flash meter red when clipping", "");


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
            pages["Samples/Waves"].AddNumberBox("Default base key", "", 0, 119, NumberBox.NumberDisplayMode.Note);
            pages["Samples/Waves"].AddBreak();
            pages["Samples/Waves"].AddLabel("Resampling");
            pages["Samples/Waves"].AddDropdown("Default wave resample mode", "", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });
            pages["Samples/Waves"].AddDropdown("Default sample resample mode", "", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });


        }

        public new void Open() {
            base.Open();
        }
        public override void Update() {
            if (windowIsOpen) {
                DoDragging();
                if (ExitButton.Clicked || cancel.Clicked) {
                    Close();
                }
                if (ok.Clicked) {
                    Close();
                }
                if (apply.Clicked) {
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

            public ConfigurationOption.Dropdown AddDropdown(string label, string description, string[] dropdownItems, bool scrollWrap = true, int dropdownPosition = -1, int dropdownWidth = -1) {
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

        class GeneralPage : Page {
            ConfigurationOption.Dropdown screenScale;
            ConfigurationOption.Checkbox flashMeterRedWhenClipping;
            public GeneralPage(Element parent) : base(parent) {
                AddLabel("General");
                screenScale = AddDropdown(
                    "Screen scale:",
                    "",
                    new string[] { "100%", "200%", "300%", "400%", "500%" },
                    false
                );
                flashMeterRedWhenClipping = AddCheckbox("Flash meter red when clipping", "");
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
                    dropdownPosition = Helpers.GetWidthOfText(name) + 5;
                }
                dropdown = new(dropdownPosition, 0, this, scrollWrap);
                if (dropdownWidth != -1) {
                    dropdown.width = dropdownWidth;
                }
                dropdown.SetMenuItems(dropdownItems);
            }

            public override void Update() {
                dropdown.Update();
            }
            public override void Draw() {
                Write(Name, 0, 3, UIColors.labelDark);
                dropdown.Draw();
            }
        }

        public class TextBox : ConfigurationOption {
            UI.Textbox textBox;

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
                    boxPosition = Helpers.GetWidthOfText(name) + 5;
                }
                textBox = new("", boxPosition, 0, boxWidth, this);
            }

            public override void Update() {
                textBox.Update();
            }
            public override void Draw() {
                Write(Name, 0, 3, UIColors.labelDark);
                textBox.Draw();
            }
        }

        public class NumberBox : ConfigurationOption {
            UI.NumberBox numberBox;

            public int Value => numberBox.Value;
            public bool ValueWasChangedInternally => numberBox.ValueWasChangedInternally;
            public NumberBox(int y, string name, string description, ConfigurationDialog.Page parent, int numberBoxPosition = -1, int numberBoxWidth = -1) : base(y, 13, name, description, parent) {
                if (numberBoxPosition == -1) {
                    numberBoxPosition = Helpers.GetWidthOfText(name) + 5;
                }
                numberBox = new("", numberBoxPosition, 0, this);
                if (numberBoxWidth != -1) {
                    numberBox.width = numberBoxWidth;
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
                Write(Name, 0, 3, UIColors.labelDark);
                numberBox.Draw();
            }
        }

    }
    #endregion

}
