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


namespace WaveTracker.Rendering
{
    public class PreferencesDialog : Dialog
    {
        public SpriteButton closeX;
        public Button ok, cancel, apply;
        public Button closeButton;
        OptionList pageGeneral, pageSample, pageVisualizer;
        TabGroup tabGroup;
        //public List<Option>
        public PreferencesDialog()
        {
            InitializeDialogCentered("Preferences", 290, 336);
            closeX = newCloseButton();
            apply = newBottomButton("Apply", this);
            cancel = newBottomButton("Cancel", this);
            ok = newBottomButton("OK", this);
            tabGroup = new TabGroup(4, 12, this);
            tabGroup.AddTab("General", false);
            tabGroup.AddTab("Samples/Waves", false);
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

            pageVisualizer = new OptionList(12, 30, width - 24, this);
            pageVisualizer.AddLabel("Visualizer: Piano");
            pageVisualizer.AddNumberBox("Note speed:", 1, 20);
            pageVisualizer.getLastOption.description = "How fast notes scroll by in the piano roll, lower values are slower.";
            pageVisualizer.AddCheckbox("Change note width by volume");
            pageVisualizer.getLastOption.description = "If this is enabled, notes get thinner as they get softer";
            pageVisualizer.AddCheckbox("Change note opacity by volume");
            pageVisualizer.getLastOption.description = "If this is enabled, notes fade out as they get softer";
            pageVisualizer.AddBreak();
            pageVisualizer.AddLabel("Visualizer: Oscilloscopes");
            pageVisualizer.AddNumberBox("Wave zoom:", 50, 200);
            pageVisualizer.getLastOption.description = "Determines how far zoomed in the waves in the oscilloscope are.";
            pageVisualizer.AddCheckbox("Colorful waves");
            pageVisualizer.getLastOption.description = "If this is enabled, each oscilloscope window will use the same color as their notes in the piano roll";
            pageVisualizer.AddDropdown("Wave thickness", new string[] { "Thin", "Medium", "Thick" });
            pageVisualizer.getLastOption.description = "Determines the thickness at which the waves are drawn in the oscilloscope";


        }

        public new void Open()
        {
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
            pageVisualizer.GetOption(6).SetValue(Preferences.profile.visualizerScopeZoom);
            pageVisualizer.GetOption(7).SetValue(Preferences.profile.visualizerScopeColors);
            pageVisualizer.GetOption(8).SetValue(Preferences.profile.visualizerScopeThickness);

            base.Open();
        }

        public void Update()
        {
            if (enabled)
            {
                if (closeX.Clicked || cancel.Clicked)
                {
                    Close();
                }
                if (ok.Clicked)
                {
                    ApplyChanges();
                    Close();
                }
                if (apply.Clicked)
                {
                    ApplyChanges();
                }
                tabGroup.Update();
                if (tabGroup.selected == 0)
                    pageGeneral.Update();
                if (tabGroup.selected == 1)
                    pageSample.Update();
                if (tabGroup.selected == 2)
                    pageVisualizer.Update();
            }
        }

        public void Draw()
        {
            if (enabled)
            {
                DrawDialog();
                tabGroup.Draw();
                DrawRoundedRect(tabGroup.x, tabGroup.y + 13, width - 8, height - 20 - 24, Color.White);
                closeX.Draw();
                apply.Draw();
                cancel.Draw();
                ok.Draw();
                int rectStart = pageGeneral.width + 6 + pageGeneral.x;
                rectStart = pageGeneral.x - 2;

                int rectWidth = width - rectStart - 8;
                int rectstartY = pageGeneral.y + pageGeneral.getListHeight() + 10;

                OptionList activePage;
                if (tabGroup.selected == 0)
                {
                    pageGeneral.Draw();
                    activePage = pageGeneral;
                }
                else if (tabGroup.selected == 1)
                {
                    pageSample.Draw();
                    activePage = pageSample;
                }
                else
                {
                    pageVisualizer.Draw();
                    activePage = pageVisualizer;
                }
                //DrawRect(rectStart, rectstartY - 5, rectWidth, 1, UIColors.labelLight);
                if (activePage.HoveredDescription != "")
                    WriteMultiline("Description: ", rectStart, rectstartY, rectWidth - 4, UIColors.labelLight);
                WriteMultiline("" + activePage.HoveredDescription, rectStart, rectstartY + 10, rectWidth - 4, UIColors.label);
            }
        }

        public void ApplyChanges()
        {
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

            Preferences.profile.visualizerPianoSpeed = pageVisualizer.GetOption(1).GetValueInt();
            Preferences.profile.visualizerPianoChangeWidth = pageVisualizer.GetOption(2).GetValueBool();
            Preferences.profile.visualizerPianoFade = pageVisualizer.GetOption(3).GetValueBool();
            Preferences.profile.visualizerScopeZoom = pageVisualizer.GetOption(6).GetValueInt();
            Preferences.profile.visualizerScopeColors = pageVisualizer.GetOption(7).GetValueBool();
            Preferences.profile.visualizerScopeThickness = pageVisualizer.GetOption(8).GetValueInt();
            Preferences.SaveToFile();
        }
    }

    public class OptionList : Element
    {
        bool enabled;
        public int width;
        List<Option> options;
        public string HoveredDescription { get; private set; }
        public void AddNumberBox(string label, int min, int max)
        {
            Opt_Number n = new Opt_Number(label, min, max, 0, getListHeight(), width, 16, this);
            options.Add(n);
        }
        public void AddCheckbox(string label)
        {
            Opt_Checkbox n = new Opt_Checkbox(label, 0, getListHeight(), width, 16, this);
            options.Add(n);
        }
        public void AddLabel(string label)
        {
            Opt_Label n = new Opt_Label(label, 0, getListHeight(), width, 11, this);
            options.Add(n);
        }
        public void AddDropdown(string label, string[] items, bool placeNextToLabel = false, int customMargin = 0, int customWidth = 0)
        {
            Opt_Dropdown n = new Opt_Dropdown(label, items, 0, getListHeight(), width, 16, this, placeNextToLabel, customMargin, customWidth);
            options.Add(n);
        }
        public void AddBreak()
        {
            Opt_Label n = new Opt_Label("", 0, getListHeight(), width, 6, this);
            options.Add(n);
        }


        public int getListHeight()
        {
            int y = 0;
            foreach (Option option in options)
            {
                y += option.optionHeight;
            }
            return y;
        }
        public Option getLastOption => options.Count > 0 ? options[options.Count - 1] : null;
        public Option GetOption(int index) { return options[index]; }

        public OptionList(int x, int y, int width, Element parent)
        {
            this.x = x;
            this.y = y;
            enabled = true;
            this.width = width;
            SetParent(parent);
            options = new List<Option>();
        }

        public void Update()
        {
            if (enabled)
            {
                HoveredDescription = "";
                foreach (Option option in options)
                {
                    if (option.IsHovered)
                        HoveredDescription = option.description + "";
                    option.Update();
                }
            }
        }

        public void Draw()
        {
            if (enabled)
            {
                for (int i = options.Count - 1; i >= 0; i--)
                {
                    options[i].Draw();
                }
            }
        }
    }

    public abstract class Option : Element
    {
        protected const int padding = 4;
        public string label;
        public string description = "";
        public int optionHeight = 16;
        public int optionWidth;
        public bool IsHovered => inFocus && MouseX >= 0 && MouseY >= 0 && MouseX < optionWidth && MouseY < optionHeight;

        public abstract void SetValue(int val);
        public abstract void SetValue(bool val);
        public abstract bool GetValueBool();
        public abstract int GetValueInt();
        public abstract void Update();
        public abstract void Draw();
    }

    public class Opt_Number : Option
    {
        public int Value => numbox.Value;
        public NumberBox numbox;
        public Opt_Number(string label, int min, int max, int x, int y, int width, int height, Element parent)
        {
            this.label = label;
            optionHeight = height;
            optionWidth = width;
            this.x = x;
            this.y = y;
            SetParent(parent);
            int maxWidth = Math.Max(Helpers.getWidthOfText(min + "wwww"), Helpers.getWidthOfText(max + "wwww"));
            numbox = new NumberBox(label, 0, (optionHeight - 13) / 2, width, width - Helpers.getWidthOfText(label) - 10, this);
            numbox.SetValueLimits(min, max);
        }
        public override bool GetValueBool()
        {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override int GetValueInt()
        {
            return Value;
        }
        public override void SetValue(int val)
        {
            numbox.Value = val;
        }
        public override void SetValue(bool val)
        {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override void Update()
        {
            numbox.Update();
        }

        public override void Draw()
        {
            if (IsHovered)
            {
                DrawRect(-padding, 0, optionWidth + padding, optionHeight, UIColors.selectionLight);
            }
            numbox.Draw();
        }
    }

    public class Opt_Checkbox : Option
    {
        public bool Value => checkbox.Value;
        public CheckboxLabeled checkbox;
        public Opt_Checkbox(string label, int x, int y, int width, int height, Element parent)
        {
            this.label = label;
            this.x = x;
            this.y = y;
            optionHeight = height;
            optionWidth = width;
            SetParent(parent);
            checkbox = new CheckboxLabeled(label, 0, 0, width, this);
            checkbox.height = optionHeight;
        }

        public override void Update()
        {
            checkbox.Update();
        }
        public override bool GetValueBool()
        {
            return checkbox.Value;
        }
        public override int GetValueInt()
        {
            return checkbox.Value ? 1 : 0;
        }
        public override void SetValue(int val)
        {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override void SetValue(bool val)
        {
            checkbox.Value = val;
        }
        public override void Draw()
        {
            if (IsHovered)
            {
                DrawRect(-padding, 0, optionWidth + padding, optionHeight, UIColors.selectionLight);
            }
            checkbox.Draw();
        }
    }

    public class Opt_Dropdown : Option
    {
        public int Value => dropdown.Value;
        public Dropdown dropdown;
        public Opt_Dropdown(string label, string[] items, int x, int y, int width, int height, Element parent, bool nextToLabel = false, int customMargin = 0, int customWidth = 0)
        {
            this.label = label;
            this.x = x;
            this.y = y;
            optionHeight = height;
            optionWidth = width;
            SetParent(parent);
            dropdown = new Dropdown(0, (height - 13) / 2, this);
            dropdown.SetMenuItems(items);
            if (customWidth > 0)
                dropdown.width = customWidth;
            if (customMargin > 0)
            {
                dropdown.x = customMargin;
            }
            else
            {
                if (nextToLabel)
                    dropdown.x = Helpers.getWidthOfText(label) + 8;
                else
                    dropdown.x = width - dropdown.width;
            }

        }
        public override bool GetValueBool()
        {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override int GetValueInt()
        {
            return dropdown.Value;
        }
        public override void SetValue(int val)
        {
            dropdown.Value = val;
        }
        public override void SetValue(bool val)
        {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override void Update()
        {
            dropdown.Update();
        }

        public override void Draw()
        {
            if (IsHovered)
            {
                DrawRect(-padding, 0, optionWidth + padding, optionHeight, UIColors.selectionLight);
            }
            Write(label, 0, optionHeight / 2 - 4, UIColors.labelDark);
            dropdown.Draw();
        }
    }

    public class Opt_Label : Option
    {
        int labelInset = 4;
        int barpadding = 3;

        public bool Value => numbox.Value;
        public SpriteToggle numbox;
        public Opt_Label(string label, int x, int y, int width, int height, Element parent)
        {
            this.label = label;
            this.x = x;
            this.y = y;
            optionWidth = width;
            optionHeight = height;
            //labelInset = (width - Helpers.getWidthOfText(label)) / 2;
            SetParent(parent);

            //numbox = new SpriteToggle();
        }

        public override void Update()
        {

        }
        public override bool GetValueBool()
        {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override int GetValueInt()
        {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override void SetValue(int val)
        {
            throw new Exception("Option has the wrong data type, cannot call int");
        }
        public override void SetValue(bool val)
        {
            throw new Exception("Option has the wrong data type, cannot call bool");
        }
        public override void Draw()
        {
            if (label == "")
                return;
            if (label == "--")
            {
                DrawRect(-padding, optionHeight / 2 - 1, padding + optionWidth, 1, UIColors.labelLight);

            }
            else
            {
                int textWidth = Helpers.getWidthOfText(label);
                DrawRect(-padding, optionHeight / 2 - 1, padding + labelInset - barpadding, 1, UIColors.labelLight);
                DrawRect(textWidth + barpadding + labelInset, optionHeight / 2 - 1, optionWidth - textWidth - barpadding - labelInset, 1, UIColors.labelLight);
                Write(label, labelInset, optionHeight / 2 - 4, UIColors.labelLight);
            }

        }
    }
}
