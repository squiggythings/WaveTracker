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

        public ConfigurationDialog() : base("Preferences...", 376, 290) {
            apply = AddNewBottomButton("Apply", this);
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
        }
        public override void Update() {

        }

        public new void Draw() {
            base.Draw();
        }


        #region Page definitions
        public class Page : Clickable {
            protected List<ConfigurationOption> options;
            protected int ypos = 16;
            public Page(Element parent) {
                width = 289;
                height = 258;
                SetParent(parent);
            }

            public virtual void Draw() {
                foreach(ConfigurationOption option in options) {
                    option.Draw();
                }
            }

            public virtual void Update() {
                foreach (ConfigurationOption option in options) {
                    option.Draw();
                }
            }

            protected ConfigurationOption.Checkbox AddCheckbox(string label, string description) {
                ypos += 1;
                ConfigurationOption.Checkbox ret = new ConfigurationOption.Checkbox(ypos, label, description, this);
                ypos += ret.height;
                ypos += 1;
                options.Add(ret);
                return ret;
            }
            protected ConfigurationOption.Label AddLabel(string label) {
                ypos += 1;
                ConfigurationOption.Label ret = new ConfigurationOption.Label(ypos, label, this);
                ypos += ret.height;
                ypos += 1;
                options.Add(ret);
                return ret;
            }

            void AddBreak() {
                ypos += 5;
            }

            protected ConfigurationOption.Dropdown AddDropdown(string label, string description, string[] dropdownItems, bool scrollWrap = true, int dropdownPosition = -1, int dropdownWidth = -1) {
                ypos += 1;
                ConfigurationOption.Dropdown ret = new ConfigurationOption.Dropdown(ypos, label, description, dropdownItems, this, scrollWrap, dropdownPosition, dropdownWidth);
                ypos += ret.height;
                ypos += 1;
                options.Add(ret);
                return ret;
            }

            protected ConfigurationOption.NumberBox AddNumberBox(string label, string description, int boxPosition = -1, int boxWidth = -1) {
                ypos += 1;
                ConfigurationOption.NumberBox ret = new ConfigurationOption.NumberBox(ypos, label, description, this, boxPosition, boxWidth);
                ypos += ret.height;
                ypos += 1;
                options.Add(ret);
                return ret;
            }

            protected ConfigurationOption.TextBox AddTextbox(string label, string description, int boxWidth, int boxPosition = -1) {
                ypos += 1;
                ConfigurationOption.TextBox ret = new ConfigurationOption.TextBox(ypos, boxWidth, label, description, this, boxPosition);
                ypos += ret.height;
                ypos += 1;
                options.Add(ret);
                return ret;
            }
        }

        class GeneralPage : Page {
            ConfigurationOption.Dropdown screenScale;
            public GeneralPage(Element parent) : base(parent) {
                screenScale = AddDropdown(
                    "Screen scale:",
                    "",
                    new string[] { "100%", "200%", "300%", "400%", "500%" },
                    false
                );
            }

            public override void Update() {

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

        public virtual void Draw() { }

        public class Label : ConfigurationOption {

            public Label(int y, string name, ConfigurationDialog.Page parent) : base(y, 9, name, "", parent) {

            }

            public override void Draw() {
                Write(Name, 4, 2, UIColors.labelLight);

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
            public bool ValueWasChangedInternally => checkbox.ValueWasChangedInternally;
            public Checkbox(int y, string name, string description, ConfigurationDialog.Page parent) : base(y, 13, name, description, parent) {
                checkbox = new(2, 2, this);
            }
            public void Update() {
                checkbox.Update();
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

            public void Update() {
                dropdown.Update();
            }
            public override void Draw() {
                Write(Name, 0, 2, UIColors.labelDark);
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

            public void Update() {
                textBox.Update();
            }
            public override void Draw() {
                Write(Name, 0, 2, UIColors.labelDark);
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

            public void Update() {
                numberBox.Update();
            }
            public override void Draw() {
                Write(Name, 0, 2, UIColors.labelDark);
                numberBox.Draw();
            }
        }

    }
    #endregion

}
