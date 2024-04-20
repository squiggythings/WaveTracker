using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.UI;


namespace WaveTracker.UI {
    public class PreferencesDialog : Dialog {
        public Button ok, cancel, apply;
        public Button closeButton;
        OptionList pageGeneral, pageSample, pageAppearance, pageVisualizer;
        TabGroup tabGroup;
        //public List<Option>
        public PreferencesDialog() : base("Preferences", 290, 336) {
            apply = AddNewBottomButton("Apply", this);
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            tabGroup = new TabGroup(4, 12, this);
            tabGroup.AddTab("General", false);
            tabGroup.AddTab("Samples/Waves", false);
            //tabGroup.AddTab("Appearance", false);
            tabGroup.AddTab("Visualizer", false);

            pageGeneral = new OptionList(12, 30, width - 24, this);
            pageGeneral.AddLabel("Frame Editor");
            pageGeneral.AddCheckbox("Show row numbers in hex");
            pageGeneral.getLastOption.description = "Display row numbers in hexadecimal instead of decimal";
            pageGeneral.AddCheckbox("Show note off and release as text");
            pageGeneral.getLastOption.description = "Display note off and release events as OFF and REL";
            pageGeneral.AddCheckbox("Fade volume column");
            pageGeneral.getLastOption.description = "Fade items in the volume column according to their value";
            pageGeneral.AddCheckbox("Ignore step when moving");
            pageGeneral.getLastOption.description = "Ignore the step value in edit settings when moving the cursor. Only use it when inputting notes.";
            pageGeneral.AddCheckbox("Key repeat");
            pageGeneral.getLastOption.description = "Enable key repetition when inputting notes and values";
            pageGeneral.AddCheckbox("Restore channel state on playback");
            pageGeneral.getLastOption.description = "Reconstruct the current channel's state from previous frames upon playing";
            pageGeneral.AddDropdown("Page jump amount", new string[] { "2", "4", "8", "16" }, true);
            pageGeneral.getLastOption.description = "How many rows the cursor jumps when scrolling";
            pageGeneral.AddBreak();
            pageGeneral.AddLabel("Metering");
            pageGeneral.AddDropdown("Oscilloscope mode", new string[] { "Mono", "Stereo: split", "Stereo: overlap" });
            pageGeneral.getLastOption.description = "Mono: Display a single oscilloscope. Stereo-split: Display 2 oscilloscopes for left and right. Stereo-overlap: Overlap left and right in a single oscilloscope, highlighting the difference between them.";
            pageGeneral.AddDropdown("Meter decay rate", new string[] { "Slow", "Medium", "Fast" });
            pageGeneral.getLastOption.description = "Determines how responsive the volume meters will be. Fast is very responsive but not very consistent; slow is more consistent but less responsive.";
            pageGeneral.AddDropdown("Meter color mode", new string[] { "Flat", "Gradient", });
            pageGeneral.getLastOption.description = "Flat: the meter is one color. Gradient: the meter becomes more red as the signal is louder ";
            pageGeneral.AddCheckbox("Flash meter red when clipping");
            pageGeneral.getLastOption.description = "If output is clipping, make the whole volume meter red.";

            pageSample = new OptionList(12, 30, width - 24, this);
            pageSample.AddLabel("Sample settings");
            pageSample.AddCheckbox("Normalize samples on import");
            pageSample.getLastOption.description = "Automatically set a sample's volume to be as loud as possible when importing";
            pageSample.AddCheckbox("Trim sample silence on import");
            pageSample.getLastOption.description = "Automatically trim any silence before and after a sample when importing";
            pageSample.AddCheckbox("Resample to 44.1 kHz on import");
            pageSample.getLastOption.description = "If a sample is not 44.1 kHz, resample it to preserve its pitch.";
            pageSample.AddCheckbox("Preview samples in browser");
            pageSample.getLastOption.description = "Plays audio files upon clicking on them in the sample browser.";
            pageSample.AddCheckbox("Include samples in visualizer by default");
            pageSample.getLastOption.description = "Defaults to include any newly imported sample in the visualizer";
            pageSample.AddNumberBox("Default base key:", 0, 119);
            pageSample.getLastOption.description = "The base key value of any new sample";
            pageSample.AddBreak();
            pageSample.AddLabel("Resampling");
            pageSample.AddDropdown("Default wave resample mode", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });
            pageSample.getLastOption.description = "The default resampling algorithm that waves in new songs will have";
            pageSample.AddDropdown("Default sample resample mode", new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });
            pageSample.getLastOption.description = "The default resampling algorithm that new samples will have";

            //pageAppearance = new OptionList(12, 30, width - 24 - 4, this);
            //pageAppearance.AddLabel("Color Theme");
            //pageAppearance.AddColorBox("Row Background", false);
            //pageAppearance.AddColorBox("Highlighted Row", false);
            //pageAppearance.AddColorBox("Sub-Highlighted Row", false);
            //pageAppearance.AddColorBox("Current Row", false);
            //pageAppearance.AddColorBox("Current Row (Editing)", false);
            //pageAppearance.AddColorBox("Current Row (Playback)", false);
            //pageAppearance.AddNumberBox("Empty Row Text Opacity", 0, 100, false);
            //pageAppearance.AddBreak();
            //pageAppearance.AddColorBox("Pattern Text", false);
            //pageAppearance.AddColorBox("Highlighted Pattern Text", false);
            //pageAppearance.AddColorBox("Sub-Highlighted Pattern Text", false);
            //pageAppearance.AddColorBox("Current Row Text", false);
            //pageAppearance.AddColorBox("Current Row Text (Editing)", false);
            //pageAppearance.AddColorBox("Current Row Text (Playback)", false);
            //pageAppearance.AddBreak();
            //pageAppearance.AddColorBox("Instrument Column (Wave)", false);
            //pageAppearance.AddColorBox("Instrument Column (Sample)", false);
            //pageAppearance.AddColorBox("Volume Column", false);
            //pageAppearance.AddColorBox("Effect Column", false);
            //pageAppearance.AddColorBox("Effect Parameter", false);
            //pageAppearance.AddBreak();
            //pageAppearance.AddColorBox("Cursor", false);
            //pageAppearance.AddColorBox("Selection", false);
            //pageAppearance.AddColorBox("Row Separator", false);


            pageVisualizer = new OptionList(12, 30, width - 24, this);
            pageVisualizer.AddLabel("Visualizer: Piano");
            pageVisualizer.AddNumberBox("Note speed:", 1, 20);
            pageVisualizer.getLastOption.description = "How fast notes scroll by in the piano roll, lower values are slower.";
            pageVisualizer.AddCheckbox("Change note width by volume");
            pageVisualizer.getLastOption.description = "If this is enabled, notes get thinner as they get softer";
            pageVisualizer.AddCheckbox("Change note opacity by volume");
            pageVisualizer.getLastOption.description = "If this is enabled, notes fade out as they get softer";
            pageVisualizer.AddCheckbox("Highlight Pressed Keys");
            pageVisualizer.getLastOption.description = "If this is enabled, currently playing notes will be highlighted on the keyboard itself.";
            pageVisualizer.AddBreak();
            pageVisualizer.AddLabel("Visualizer: Oscilloscopes");
            pageVisualizer.AddNumberBox("Wave zoom:", 50, 200);
            pageVisualizer.getLastOption.description = "Determines how far zoomed in the waves in the oscilloscope are.";
            pageVisualizer.AddCheckbox("Colorful waves");
            pageVisualizer.getLastOption.description = "If this is enabled, each oscilloscope window will use the same color as their notes in the piano roll";
            pageVisualizer.AddDropdown("Wave thickness", new string[] { "Thin", "Medium", "Thick" });
            pageVisualizer.getLastOption.description = "Determines the thickness at which the waves are drawn in the oscilloscope";
            pageVisualizer.AddDropdown("Crosshairs", new string[] { "None", "Horizontal", "Horizontal + Vertical" });
            pageVisualizer.getLastOption.description = "Determines the thickness at which the waves are drawn in the oscilloscope";
            pageVisualizer.AddCheckbox("Oscilloscope borders");
            pageVisualizer.getLastOption.description = "Draws white borders around each oscilloscope channel";


        }

        public new void Open() {
            pageGeneral.GetOption(1).SetValue(Preferences.profile.showRowNumbersInHex);
            pageGeneral.GetOption(2).SetValue(Preferences.profile.showNoteCutAndReleaseAsText);
            pageGeneral.GetOption(3).SetValue(Preferences.profile.fadeVolumeColumn);
            pageGeneral.GetOption(4).SetValue(Preferences.profile.ignoreStepWhenMoving);
            pageGeneral.GetOption(5).SetValue(Preferences.profile.keyRepeat);
            pageGeneral.GetOption(6).SetValue(Preferences.profile.restoreChannelState);
            pageGeneral.GetOption(7).SetValue((int)Math.Log2(Preferences.profile.pageJumpAmount) - 1);
            pageGeneral.GetOption(10).SetValue(Preferences.profile.oscilloscopeMode);
            pageGeneral.GetOption(11).SetValue(Preferences.profile.meterDecaySpeed);
            pageGeneral.GetOption(12).SetValue(Preferences.profile.meterColorMode);
            pageGeneral.GetOption(13).SetValue(Preferences.profile.meterFlashWhenClipping);

            pageSample.GetOption(1).SetValue(Preferences.profile.automaticallyNormalizeSamples);
            pageSample.GetOption(2).SetValue(Preferences.profile.automaticallyTrimSamples);
            pageSample.GetOption(3).SetValue(Preferences.profile.automaticallyResampleSamples);
            pageSample.GetOption(4).SetValue(Preferences.profile.previewSamples);
            pageSample.GetOption(5).SetValue(Preferences.profile.includeSamplesInVisualizer);
            pageSample.GetOption(6).SetValue(Preferences.profile.defaultBaseKey);
            pageSample.GetOption(9).SetValue(Preferences.profile.defaultResampleWave);
            pageSample.GetOption(10).SetValue(Preferences.profile.defaultResampleSample);

            pageVisualizer.GetOption(1).SetValue(Preferences.profile.visualizerPianoSpeed);
            pageVisualizer.GetOption(2).SetValue(Preferences.profile.visualizerPianoChangeWidth);
            pageVisualizer.GetOption(3).SetValue(Preferences.profile.visualizerPianoFade);
            pageVisualizer.GetOption(4).SetValue(Preferences.profile.visualizerHighlightKeys);
            pageVisualizer.GetOption(7).SetValue(Preferences.profile.visualizerScopeZoom);
            pageVisualizer.GetOption(8).SetValue(Preferences.profile.visualizerScopeColors);
            pageVisualizer.GetOption(9).SetValue(Preferences.profile.visualizerScopeThickness);
            pageVisualizer.GetOption(10).SetValue(Preferences.profile.visualizerScopeCrosshairs);
            pageVisualizer.GetOption(11).SetValue(Preferences.profile.visualizerScopeBorders);

            base.Open();
        }

        public override void Update() {
            if (windowIsOpen) {
                DoDragging();
                if (ExitButton.Clicked || cancel.Clicked) {
                    Close();
                }
                if (ok.Clicked) {
                    ApplyChanges();
                    Close();
                }
                if (apply.Clicked) {
                    ApplyChanges();
                }
                tabGroup.Update();
                if (tabGroup.SelectedTabIndex == 0)
                    pageGeneral.Update();
                if (tabGroup.SelectedTabIndex == 1)
                    pageSample.Update();
                //if (tabGroup.selected == 2)
                //    pageAppearance.Update();
                if (tabGroup.SelectedTabIndex == 2)
                    pageVisualizer.Update();
            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                DoDragging();
                tabGroup.Draw();
                DrawRoundedRect(tabGroup.x, tabGroup.y + 13, width - 8, height - 20 - 24, Color.White);
                int rectStart = pageGeneral.x - 2;

                int rectWidth = width - rectStart - 8;
                int rectstartY = pageGeneral.y + pageGeneral.getListHeight() + 10;

                OptionList activePage;
                if (tabGroup.SelectedTabIndex == 0) {
                    pageGeneral.Draw();
                    activePage = pageGeneral;
                }
                else if (tabGroup.SelectedTabIndex == 1) {
                    pageSample.Draw();
                    activePage = pageSample;
                }
                //else if (tabGroup.selected == 2)
                //{
                //    pageAppearance.Draw();
                //    activePage = pageAppearance;
                //}
                else {
                    pageVisualizer.Draw();
                    activePage = pageVisualizer;
                }
                //DrawRect(rectStart, rectstartY - 5, rectWidth, 1, UIColors.labelLight);
                if (activePage.HoveredDescription != "")
                    WriteMultiline("Description: ", rectStart, rectstartY, rectWidth - 4, UIColors.labelLight);
                WriteMultiline("" + activePage.HoveredDescription, rectStart, rectstartY + 10, rectWidth - 4, UIColors.label);
            }
        }

        public void ApplyChanges() {
            Preferences.profile.showRowNumbersInHex = pageGeneral.GetOption(1).GetValueBool();
            Preferences.profile.showNoteCutAndReleaseAsText = pageGeneral.GetOption(2).GetValueBool();
            Preferences.profile.fadeVolumeColumn = pageGeneral.GetOption(3).GetValueBool();
            Preferences.profile.ignoreStepWhenMoving = pageGeneral.GetOption(4).GetValueBool();
            Preferences.profile.keyRepeat = pageGeneral.GetOption(5).GetValueBool();
            Preferences.profile.restoreChannelState = pageGeneral.GetOption(6).GetValueBool();
            Preferences.profile.pageJumpAmount = (int)Math.Pow(2, pageGeneral.GetOption(7).GetValueInt() + 1);
            Preferences.profile.oscilloscopeMode = pageGeneral.GetOption(10).GetValueInt();
            Preferences.profile.meterDecaySpeed = pageGeneral.GetOption(11).GetValueInt();
            Preferences.profile.meterColorMode = pageGeneral.GetOption(12).GetValueInt();
            Preferences.profile.meterFlashWhenClipping = pageGeneral.GetOption(13).GetValueBool();

            Preferences.profile.automaticallyNormalizeSamples = pageSample.GetOption(1).GetValueBool();
            Preferences.profile.automaticallyTrimSamples = pageSample.GetOption(2).GetValueBool();
            Preferences.profile.automaticallyResampleSamples = pageSample.GetOption(3).GetValueBool();
            Preferences.profile.previewSamples = pageSample.GetOption(4).GetValueBool();
            Preferences.profile.includeSamplesInVisualizer = pageSample.GetOption(5).GetValueBool();
            Preferences.profile.defaultBaseKey = pageSample.GetOption(6).GetValueInt();
            Preferences.profile.defaultResampleWave = pageSample.GetOption(9).GetValueInt();
            Preferences.profile.defaultResampleSample = pageSample.GetOption(10).GetValueInt();

            // TODO add color theme

            Preferences.profile.visualizerPianoSpeed = pageVisualizer.GetOption(1).GetValueInt();
            Preferences.profile.visualizerPianoChangeWidth = pageVisualizer.GetOption(2).GetValueBool();
            Preferences.profile.visualizerPianoFade = pageVisualizer.GetOption(3).GetValueBool();
            Preferences.profile.visualizerHighlightKeys = pageVisualizer.GetOption(4).GetValueBool();
            Preferences.profile.visualizerScopeZoom = pageVisualizer.GetOption(7).GetValueInt();
            Preferences.profile.visualizerScopeColors = pageVisualizer.GetOption(8).GetValueBool();
            Preferences.profile.visualizerScopeThickness = pageVisualizer.GetOption(9).GetValueInt();
            Preferences.profile.visualizerScopeCrosshairs = pageVisualizer.GetOption(10).GetValueInt();
            Preferences.profile.visualizerScopeBorders = pageVisualizer.GetOption(11).GetValueBool();
            Preferences.SaveToFile();
        }
    }

    public class OptionList : Element {
        bool enabled;
        public int width;
        Scrollbar scrollbar;
        List<Option> options;
        public string HoveredDescription { get; private set; }
        public void AddNumberBox(string label, int min, int max, bool scrollingEnabled = true) {
            Opt_Number n = new Opt_Number(label, min, max, scrollingEnabled, 0, getListHeight(), width, 16, this);
            options.Add(n);
        }
        public void AddCheckbox(string label) {
            Opt_Checkbox n = new Opt_Checkbox(label, 0, getListHeight(), width, 16, this);
            options.Add(n);
        }
        public void AddLabel(string label) {
            Opt_Label n = new Opt_Label(label, 0, getListHeight(), width, 16, this);
            options.Add(n);
        }
        public void AddDropdown(string label, string[] items, bool placeNextToLabel = false, int customMargin = 0, int customWidth = 0) {
            Opt_Dropdown n = new Opt_Dropdown(label, items, 0, getListHeight(), width, 16, this, placeNextToLabel, customMargin, customWidth);
            options.Add(n);
        }
        public void AddColorBox(string label, bool placeNextToLabel = false, int customMargin = 0) {
            Opt_Color n = new Opt_Color(label, 0, getListHeight(), width, 16, this, placeNextToLabel, customMargin);
            options.Add(n);
        }
        public void AddBreak() {
            Opt_Label n = new Opt_Label("", 0, getListHeight(), width, 6, this);
            options.Add(n);
        }


        public int getListHeight() {
            int y = 0;
            foreach (Option option in options) {
                y += option.optionHeight;
            }
            return y;
        }
        public Option getLastOption => options.Count > 0 ? options[options.Count - 1] : null;
        public Option GetOption(int index) { return options[index]; }

        public OptionList(int x, int y, int width, Element parent) {
            this.x = x;
            this.y = y;
            enabled = true;
            this.width = width;
            SetParent(parent);
            scrollbar = new Scrollbar(0, 0, width + 8, 224, this);
            options = new List<Option>();
        }

        public void Update() {
            if (enabled) {
                scrollbar.SetSize(options.Count - 1, 13);
                scrollbar.Update();
                HoveredDescription = "";
                foreach (Option option in options) {
                    if (option.IsHovered)
                        HoveredDescription = option.description + "";

                    option.y = option.origY - scrollbar.ScrollValue * 16;
                    if (option.y >= 0 && option.y < 209)
                        option.Update();
                }
            }
        }

        public void Draw() {
            if (enabled) {
                for (int i = scrollbar.ScrollValue + scrollbar.viewportSize; i >= scrollbar.ScrollValue; i--) {
                    if (i < options.Count && i >= 0)
                        options[i].Draw();
                }
                scrollbar.Draw();
            }
        }
    }

    public abstract class Option : Element {
        protected const int padding = 4;
        public string label;
        public string description = "";
        public int optionHeight = 16;
        public int optionWidth;
        public int origX, origY;
        public bool IsHovered => InFocus && MouseX >= 0 && MouseY >= 0 && MouseX < optionWidth && MouseY < optionHeight;

        public abstract void SetValue(int val);
        public abstract void SetValue(bool val);

        public abstract void SetValue(string val);
        public abstract bool GetValueBool();
        public abstract int GetValueInt();
        public abstract string GetValueString();
        public abstract void Update();
        public abstract void Draw();
    }

    public class Opt_Number : Option {
        public int Value => numbox.Value;
        public NumberBox numbox;
        public Opt_Number(string label, int min, int max, bool canScroll, int x, int y, int width, int height, Element parent) {
            this.label = label;
            optionHeight = height;
            optionWidth = width;
            this.x = x;
            this.y = y;
            origX = x;
            origY = y;
            SetParent(parent);
            int maxWidth = Helpers.GetWidthOfText(min + "wwww");
            numbox = new NumberBox(label, 0, (optionHeight - 13) / 2, width, width - Helpers.GetWidthOfText(label) - 10, this);
            numbox.SetValueLimits(min, max);
            if (!canScroll) {
                numbox.DisableScrolling();
            }
        }
        public override bool GetValueBool() {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override int GetValueInt() {
            return Value;
        }
        public override string GetValueString() {
            throw new Exception("Option has the wrong data type, cannot call string");
        }

        public override void SetValue(int val) {
            numbox.Value = val;
        }
        public override void SetValue(bool val) {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override void SetValue(string val) {
            throw new Exception("Option has the wrong data type, cannot call string");
        }

        public override void Update() {
            numbox.Update();
        }

        public override void Draw() {
            if (IsHovered) {
                DrawRect(-padding, 0, optionWidth + padding, optionHeight, UIColors.selectionLight);
            }
            numbox.Draw();
        }
    }

    public class Opt_Color : Option {
        public string Value => colorButton.HexValue;
        public ColorButton colorButton;
        public Opt_Color(string label, int x, int y, int width, int height, Element parent, bool nextToLabel = false, int customMargin = 0) {
            this.label = label;
            this.x = x;
            this.y = y;
            origX = x;
            origY = y;
            optionHeight = height;
            optionWidth = width;
            SetParent(parent);
            colorButton = new ColorButton(Color.White, 0, (height - 13) / 2, this);
            colorButton.width = 100;

            if (customMargin > 0) {
                colorButton.x = customMargin;
            }
            else {
                if (nextToLabel)
                    colorButton.x = Helpers.GetWidthOfText(label) + 8;
                else
                    colorButton.x = width - colorButton.width;
            }

        }
        public override bool GetValueBool() {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override int GetValueInt() {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override string GetValueString() {
            return colorButton.HexValue;
        }
        public override void SetValue(int val) {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override void SetValue(bool val) {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override void SetValue(string val) {
            colorButton.HexValue = val;
        }
        public override void Update() {
            colorButton.Update();
        }

        public override void Draw() {
            if (IsHovered) {
                DrawRect(-padding, 0, optionWidth + padding, optionHeight, UIColors.selectionLight);
            }
            Write(label, 0, optionHeight / 2 - 4, UIColors.labelDark);
            colorButton.Draw();
        }
    }

    public class Opt_Checkbox : Option {
        public bool Value => checkbox.Value;
        public CheckboxLabeled checkbox;
        public Opt_Checkbox(string label, int x, int y, int width, int height, Element parent) {
            this.label = label;
            this.x = x;
            this.y = y;
            origX = x;
            origY = y;
            optionHeight = height;
            optionWidth = width;
            SetParent(parent);
            checkbox = new CheckboxLabeled(label, 0, 0, width, this);
            checkbox.height = optionHeight;
        }

        public override void Update() {
            checkbox.Update();
        }
        public override bool GetValueBool() {
            return checkbox.Value;
        }
        public override int GetValueInt() {
            return checkbox.Value ? 1 : 0;
        }
        public override string GetValueString() {
            throw new Exception("Option has the wrong data type, cannot call string");
        }
        public override void SetValue(int val) {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override void SetValue(bool val) {
            checkbox.Value = val;
        }
        public override void SetValue(string val) {
            throw new Exception("Option has the wrong data type, cannot call string");
        }
        public override void Draw() {
            if (IsHovered) {
                DrawRect(-padding, 0, optionWidth + padding, optionHeight, UIColors.selectionLight);
            }
            checkbox.Draw();
        }
    }

    public class Opt_Dropdown : Option {
        public int Value => dropdown.Value;
        public Dropdown dropdown;
        public Opt_Dropdown(string label, string[] items, int x, int y, int width, int height, Element parent, bool nextToLabel = false, int customMargin = 0, int customWidth = 0) {
            this.label = label;
            this.x = x;
            this.y = y;
            origX = x;
            origY = y;
            optionHeight = height;
            optionWidth = width;
            SetParent(parent);
            dropdown = new Dropdown(0, (height - 13) / 2, this);
            dropdown.SetMenuItems(items);
            if (customWidth > 0)
                dropdown.width = customWidth;
            if (customMargin > 0) {
                dropdown.x = customMargin;
            }
            else {
                if (nextToLabel)
                    dropdown.x = Helpers.GetWidthOfText(label) + 8;
                else
                    dropdown.x = width - dropdown.width;
            }

        }
        public override bool GetValueBool() {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override int GetValueInt() {
            return dropdown.Value;
        }
        public override string GetValueString() {
            throw new Exception("Option has the wrong data type, cannot call string");
        }
        public override void SetValue(int val) {
            dropdown.Value = val;
        }
        public override void SetValue(bool val) {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override void SetValue(string val) {
            throw new Exception("Option has the wrong data type, cannot call string");
        }
        public override void Update() {
            dropdown.Update();
        }

        public override void Draw() {
            if (IsHovered) {
                DrawRect(-padding, 0, optionWidth + padding, optionHeight, UIColors.selectionLight);
            }
            Write(label, 0, optionHeight / 2 - 4, UIColors.labelDark);
            dropdown.Draw();
        }
    }

    public class Opt_Label : Option {
        int labelInset = 4;
        int barpadding = 3;

        public bool Value => numbox.Value;
        public SpriteToggle numbox;
        public Opt_Label(string label, int x, int y, int width, int height, Element parent) {
            this.label = label;
            this.x = x;
            this.y = y;
            origX = x;
            origY = y;
            optionWidth = width;
            optionHeight = height;
            //labelInset = (width - Helpers.getWidthOfText(label)) / 2;
            SetParent(parent);

            //numbox = new SpriteToggle();
        }

        public override void Update() {

        }
        public override bool GetValueBool() {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override string GetValueString() {
            throw new Exception("Option has the wrong data type, cannot call string");
        }
        public override int GetValueInt() {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override void SetValue(int val) {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override void SetValue(bool val) {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override void SetValue(string val) {
            throw new Exception("Option has the wrong data type, cannot call string");
        }
        public override void Draw() {
            if (label == "")
                return;
            if (label == "--") {
                DrawRect(-padding, optionHeight / 2 - 1, padding + optionWidth, 1, UIColors.labelLight);
            }
            else {
                int textWidth = Helpers.GetWidthOfText(label);
                DrawRect(-padding, optionHeight / 2 - 1, padding + labelInset - barpadding, 1, UIColors.labelLight);
                DrawRect(textWidth + barpadding + labelInset, optionHeight / 2 - 1, optionWidth - textWidth - barpadding - labelInset, 1, UIColors.labelLight);
                Write(label, labelInset, optionHeight / 2 - 4, UIColors.labelLight);
            }

        }
    }
}
