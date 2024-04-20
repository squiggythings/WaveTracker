using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using WaveTracker.UI;
using WaveTracker.Tracker;
using WaveTracker.Audio;

namespace WaveTracker.UI {
    public class WaveEditor : Window {
        public bool IsOpen { get { return windowIsOpen || (currentDialog != null && currentDialog.InFocus); } }
        public static bool enabled;
        public SpriteButton presetSine, presetTria, presetSaw, presetRect50, presetRect25, presetRect12, presetRand, presetClear;
        public Toggle filterNone, filterLinear, filterMix;
        public int startcooldown;
        //public SpriteButton closeButton;
        public Button bCopy, bPaste, bPhaseL, bPhaseR, bMoveUp, bMoveDown, bInvert, bMutate, bSmooth, bNormalize;
        public DropdownButton bModify;
        public Textbox waveText;
        public Dropdown ResampleDropdown;
        public int id;
        int holdPosY, holdPosX;
        static string clipboardWave = "";
        static ResamplingMode clipboardSampleMode = ResamplingMode.Mix;
        PreviewPiano piano;
        MouseRegion drawingRegion;
        bool displayAsLines;
        Dialog currentDialog;
        //MenuStrip MenuStrip { get; set; }

        Wave CurrentWave => App.CurrentModule.WaveBank[WaveBank.currentWaveID];

        int phase;
        public WaveEditor() : base("Wave Editor", 500, 270) {

            int buttonY = 23;
            int buttonX = 420;
            int buttonWidth = 64;


            bCopy = new UI.Button("Copy", buttonX, buttonY, this);
            bCopy.width = buttonWidth / 2 - 1;
            bCopy.SetTooltip("", "Copy wave settings");
            buttonY += 0;
            bPaste = new UI.Button("Paste", buttonX + 32, buttonY, this);
            bPaste.width = buttonWidth / 2 - 1;
            bPaste.SetTooltip("", "Paste wave settings");
            buttonY += 18;

            bPhaseR = new UI.Button("Phase »", buttonX, buttonY, this);
            bPhaseR.width = buttonWidth;
            bPhaseR.SetTooltip("", "Shift phase once to the right");
            buttonY += 14;
            bPhaseL = new UI.Button("Phase «", buttonX, buttonY, this);
            bPhaseL.width = buttonWidth;
            bPhaseL.SetTooltip("", "Shift phase once to the left");
            buttonY += 14;
            bMoveUp = new UI.Button("Shift Up", buttonX, buttonY, this);
            bMoveUp.width = buttonWidth;
            bMoveUp.SetTooltip("", "Raise the wave 1 step up");
            buttonY += 14;
            bMoveDown = new UI.Button("Shift Down", buttonX, buttonY, this);
            bMoveDown.width = buttonWidth;
            bMoveDown.SetTooltip("", "Lower the wave 1 step down");
            buttonY += 18;

            bInvert = new UI.Button("Invert", buttonX, buttonY, this);
            bInvert.width = buttonWidth;
            bInvert.SetTooltip("", "Invert the wave vertically");
            buttonY += 14;
            //bSmooth = new UI.Button("Smooth", buttonX, buttonY, this);
            //bSmooth.width = buttonWidth;
            //bSmooth.SetTooltip("", "Smooth out rough corners in the wave");
            //buttonY += 14;
            bMutate = new UI.Button("Mutate", buttonX, buttonY, this);
            bMutate.width = buttonWidth;
            bMutate.SetTooltip("", "Slightly randomize the wave");
            buttonY += 14;
            bNormalize = new UI.Button("Normalize", buttonX, buttonY, this);
            bNormalize.width = buttonWidth;
            bNormalize.SetTooltip("", "Make the wave maximum amplitude");
            buttonY += 18;


            bModify = new DropdownButton("Modify...", buttonX, buttonY, this);
            bModify.SetMenuItems(new string[] { "Smooth...", "Add Fuzz...", "Set Harmonic...", "Sample and hold..." });
            bModify.width = buttonWidth;


            presetSine = new SpriteButton(17, 215, 18, 12, 104, 80, this);
            presetSine.SetTooltip("Sine", "Sine wave preset");
            presetTria = new SpriteButton(36, 215, 18, 12, 122, 80, this);
            presetTria.SetTooltip("Triangle", "Triangle wave preset");
            presetSaw = new SpriteButton(55, 215, 18, 12, 140, 80, this);
            presetSaw.SetTooltip("Sawtooth", "Sawtooth wave preset");

            presetRect50 = new SpriteButton(74, 215, 18, 12, 158, 80, this);
            presetRect50.SetTooltip("Pulse 50%", "Pulse wave preset with 50% duty cycle");
            presetRect25 = new SpriteButton(93, 215, 18, 12, 176, 80, this);
            presetRect25.SetTooltip("Pulse 25%", "Pulse wave preset with 25% duty cycle");
            presetRect12 = new SpriteButton(112, 215, 18, 12, 194, 80, this);
            presetRect12.SetTooltip("Pulse 12.5%", "Pulse wave preset with 12.5% duty cycle");

            presetRand = new SpriteButton(131, 215, 18, 12, 212, 80, this);
            presetRand.SetTooltip("Random", "Create random noise");
            presetClear = new SpriteButton(150, 215, 18, 12, 230, 80, this);
            presetClear.SetTooltip("Clear", "Clear wave");

            //closeButton = new SpriteButton(490, 0, 10, 9, UI.NumberBox.buttons, 4, this);
            ExitButton.SetTooltip("Close", "Close wave editor");

            drawingRegion = new MouseRegion(17, 23, 384, 160, this);

            waveText = new UI.Textbox("", 17, 188, 384, 384, this);
            waveText.canEdit = true;
            waveText.MaxLength = 192;

            ResampleDropdown = new Dropdown(385, 215, this);
            ResampleDropdown.SetMenuItems(new string[] { "Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)" });
            piano = new PreviewPiano(10, 240, this);

            //MenuStrip = new MenuStrip(0, 9, width, this);
            //MenuStrip.AddButton("File", new Menu(new MenuItemBase[] {
            //    new MenuOption("New",null),
            //    new MenuOption("Open...",null),
            //    new MenuOption("Save",null),
            //    new MenuOption("Save As...",null),
            //    null,
            //    new MenuOption("Export as WAV...",null),
            //    null,
            //    new MenuOption("Configuration...",null),
            //    null,
            //    new SubMenu("Recent files",new MenuItemBase[] {
            //        new MenuOption("Clear",null),
            //        null,
            //        new MenuOption("1. C:\\Users\\Elias\\Music\\wavetracker\\moon2.0.wtm",null),
            //        new MenuOption("2. C:\\Users\\Elias\\Music\\wavetracker\\wtdemo2.wtm",null),
            //        new MenuOption("3. C:\\Users\\Elias\\Music\\wavetracker\\freezedraft.wtm",null),
            //        new MenuOption("4. C:\\Users\\Elias\\Music\\wavetracker\\fmcomplextro.wtm",null),
            //        new MenuOption("5. C:\\Users\\Elias\\Music\\wavetracker\\modtesting.wtm",null),
            //        new MenuOption("6. C:\\Users\\Elias\\Music\\wavetracker\\largetest2.0.wtm",null),
            //        new MenuOption("7. C:\\Users\\Elias\\Music\\wavetracker\\katamarisolo7.wtm",null),
            //        new MenuOption("8. C:\\Users\\Elias\\Music\\wavetracker\\itsmyblaster2.0.wtm",null),
            //    }),
            //    null,
            //    new MenuOption("Exit", App.ExitApplication),
            //}));
            //MenuStrip.AddButton("Edit", new Menu(new MenuItemBase[] {
            //    new MenuOption("Undo",null),
            //    new MenuOption("Redo",null),
            //    null,
            //    new MenuOption("Cut",null),
            //    new MenuOption("Copy",null),
            //    new MenuOption("Paste",null),
            //    new MenuOption("Paste and mix",null),
            //}));
            //MenuStrip.AddButton("Song", new Menu(new MenuItemBase[] {
            //    new MenuOption("Insert frame",null),
            //    new MenuOption("Remove frame",null),
            //    new MenuOption("Duplicate frame",null),
            //    new MenuOption("Duplicate frame",null),
            //    null,
            //    new MenuOption("Move frame up",null),
            //    new MenuOption("Move frame down",null),
            //}));
        }
        public void Open(int waveIndex) {
            base.Open();
            //Input.internalDialogIsOpen = true;
            startcooldown = 10;
            id = waveIndex;
            enabled = true;
            //Input.focus = this;
        }

        public new void Close() {
            enabled = false;
            base.Close();
            //Input.internalDialogIsOpen = false;
            //Input.focus = null;
        }

        public int GetPianoMouseInput() {
            if (!enabled || !InFocus)
                return -1;
            return piano.CurrentClickedNote;
        }

        bool IsMouseInCanvasBounds() {
            return InFocus && drawingRegion.IsHovered;
        }
        int CanvasPosX => drawingRegion.MouseX / 6;
        int CanvasPosY => 31 - (drawingRegion.MouseY / 5);

        void ToggleDisplayMode() {
            displayAsLines = !displayAsLines;
        }

        public void Update() {
            if (windowIsOpen) {
                DoDragging();
                //MenuStrip.Update();
                if (WaveBank.currentWaveID < 0) return;
                if (WaveBank.currentWaveID > 99) return;
                if (Input.GetKeyRepeat(Keys.Left, KeyModifier.None)) {
                    WaveBank.currentWaveID--;
                    if (WaveBank.currentWaveID < 0) {
                        WaveBank.currentWaveID += 100;
                    }
                    id = WaveBank.currentWaveID;
                }
                if (Input.GetKeyRepeat(Keys.Right, KeyModifier.None)) {
                    WaveBank.currentWaveID++;
                    if (WaveBank.currentWaveID >= 100) {
                        WaveBank.currentWaveID -= 100;
                    }
                    id = WaveBank.currentWaveID;

                }
                if (WaveBank.lastSelectedWave != id) {
                    WaveBank.lastSelectedWave = id;
                    ChannelManager.previewChannel.SetWave(WaveBank.currentWaveID);
                }

                phase++;

                ResampleDropdown.Value = (int)CurrentWave.resamplingMode;
                if (startcooldown > 0) {
                    waveText.Text = CurrentWave.ToNumberString();
                    startcooldown--;
                }
                else {
                    if (ExitButton.Clicked || Input.GetKeyDown(Keys.Escape, KeyModifier.None)) {
                        Close();
                    }
                    waveText.Update();
                    if (waveText.ValueWasChangedInternally) {
                        App.CurrentModule.SetDirty();
                    }
                    if (waveText.ValueWasChanged) {
                        CurrentWave.SetFromNumberString(waveText.Text);
                    }
                    else {
                        waveText.Text = CurrentWave.ToNumberString();
                    }

                    ResampleDropdown.Update();
                    if (ResampleDropdown.ValueWasChangedInternally) {
                        App.CurrentModule.SetDirty();
                    }
                    CurrentWave.resamplingMode = (ResamplingMode)ResampleDropdown.Value;

                    if (presetSine.Clicked) {
                        CurrentWave.SetWaveformFromString("HJKMNOQRSTUUVVVVVVVUUTSRQONMKJHGECB9875432110000000112345789BCEF");
                        App.CurrentModule.SetDirty();
                    }
                    if (presetTria.Clicked) {
                        CurrentWave.SetWaveformFromString("GHIJKLMNOPQRSTUVVUTSRQPONMLKJIHGFEDCBA98765432100123456789ABCDEF");
                        App.CurrentModule.SetDirty();
                    }
                    if (presetSaw.Clicked) {
                        CurrentWave.SetWaveformFromString("GGHHIIJJKKLLMMNNOOPPQQRRSSTTUUVV00112233445566778899AABBCCDDEEFF");
                        App.CurrentModule.SetDirty();
                    }
                    if (presetRect50.Clicked) {
                        CurrentWave.SetWaveformFromString("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV00000000000000000000000000000000");
                        App.CurrentModule.SetDirty();
                    }
                    if (presetRect25.Clicked) {
                        CurrentWave.SetWaveformFromString("VVVVVVVVVVVVVVVV000000000000000000000000000000000000000000000000");
                        App.CurrentModule.SetDirty();
                    }
                    if (presetRect12.Clicked) {
                        CurrentWave.SetWaveformFromString("VVVVVVVV00000000000000000000000000000000000000000000000000000000");
                        App.CurrentModule.SetDirty();
                    }
                    if (presetRand.Clicked) {
                        CurrentWave.Randomize();
                        App.CurrentModule.SetDirty();
                    }
                    if (presetClear.Clicked) {
                        CurrentWave.SetWaveformFromString("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG");
                        App.CurrentModule.SetDirty();
                    }
                    if (bCopy.Clicked) {
                        clipboardWave = CurrentWave.ToString();
                        clipboardSampleMode = CurrentWave.resamplingMode;
                    }
                    if (bPaste.Clicked) {
                        if (clipboardWave.Length == 64) {
                            CurrentWave.SetWaveformFromString(clipboardWave);
                            CurrentWave.resamplingMode = clipboardSampleMode;
                            App.CurrentModule.SetDirty();
                        }
                    }

                    if (bPhaseL.Clicked) {
                        CurrentWave.ShiftPhase(1);
                        App.CurrentModule.SetDirty();
                    }
                    if (bPhaseR.Clicked) {
                        CurrentWave.ShiftPhase(-1);
                        App.CurrentModule.SetDirty();
                    }
                    if (bMoveUp.Clicked) {
                        CurrentWave.Move(1);
                        App.CurrentModule.SetDirty();
                    }
                    if (bMoveDown.Clicked) {
                        CurrentWave.Move(-1);
                        App.CurrentModule.SetDirty();
                    }
                    if (bInvert.Clicked) {
                        CurrentWave.Invert();
                        App.CurrentModule.SetDirty();
                    }
                    //if (bSmooth.Clicked) {
                    //    CurrentWave.Smooth(2);
                    //    App.CurrentModule.SetDirty();
                    //}
                    if (bMutate.Clicked) {
                        CurrentWave.MutateHarmonics();
                        App.CurrentModule.SetDirty();
                    }
                    if (bNormalize.Clicked) {
                        CurrentWave.Normalize();
                        App.CurrentModule.SetDirty();
                    }
                    bModify.Update();
                    if (bModify.SelectedAnItem) {
                        switch (bModify.SelectedIndex) {
                            case 0:
                                currentDialog = Dialogs.waveSmoothDialog;
                                Dialogs.waveSmoothDialog.Open(CurrentWave);
                                break;
                            case 1:
                                currentDialog = Dialogs.waveAddFuzzDialog;
                                Dialogs.waveAddFuzzDialog.Open(CurrentWave);
                                break;
                            case 2:
                                currentDialog = Dialogs.waveSyncDialog;
                                Dialogs.waveSyncDialog.Open(CurrentWave);
                                break;
                            case 3:
                                currentDialog = Dialogs.waveSampleAndHoldDialog;
                                Dialogs.waveSampleAndHoldDialog.Open(CurrentWave);
                                break;
                        }
                        App.CurrentModule.SetDirty();
                    }
                    if (IsMouseInCanvasBounds()) {
                        if (drawingRegion.RightClicked) {
                            ContextMenu.Open(new Menu(
                                new MenuItemBase[] {
                                    new MenuOption("Show as grid",ToggleDisplayMode,displayAsLines),
                                    new MenuOption("Show as line",ToggleDisplayMode,!displayAsLines),
                                }
                            ));
                        }
                        if (drawingRegion.DidClickInRegionM(KeyModifier._Any)) {
                            holdPosX = CanvasPosX;
                            holdPosY = CanvasPosY;
                        }


                        if (drawingRegion.DidClickInRegionM(KeyModifier.None)) {
                            CurrentWave.samples[CanvasPosX] = (byte)CanvasPosY;
                            App.CurrentModule.SetDirty();
                        }
                        if (drawingRegion.ClickedM(KeyModifier.Shift)) {
                            int diff = Math.Abs(holdPosX - CanvasPosX);
                            if (diff > 0) {
                                if (holdPosX < CanvasPosX) {
                                    for (int i = holdPosX; i <= CanvasPosX; ++i) {
                                        CurrentWave.samples[i] = (byte)Math.Round(MathHelper.Lerp(holdPosY, CanvasPosY, (float)(i - holdPosX) / diff));
                                    }
                                }
                                else {
                                    for (int i = CanvasPosX; i <= holdPosX; ++i) {
                                        CurrentWave.samples[i] = (byte)Math.Round(MathHelper.Lerp(CanvasPosY, holdPosY, (float)(i - CanvasPosX) / diff));
                                    }
                                }
                                App.CurrentModule.SetDirty();
                            }
                            else {
                                CurrentWave.samples[CanvasPosX] = (byte)CanvasPosY;
                                App.CurrentModule.SetDirty();
                            }
                        }
                    }
                }
            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                name = "Edit Wave " + id.ToString("D2");
                base.Draw();

                Write("Draw Wave", 17, 14, UIColors.label);
                DrawSprite(16, 22, new Rectangle(0, 144, 386, 162));
                //DrawRect(-x, -y, 960, 600, Helpers.Alpha(Color.Black, 90));
                //DrawSprite( 0, 0, new Rectangle(0, 60, 500, 270));
                //Write("Edit Wave " + id.ToString("D2"), 4, 1, new Color(64, 72, 115));

                Write("Presets", 17, 205, UIColors.label);
                presetSine.Draw();
                presetTria.Draw();
                presetRect50.Draw();
                presetSaw.Draw();
                presetRect25.Draw();
                presetRect12.Draw();
                presetClear.Draw();
                presetRand.Draw();
                //filterNone.Draw();
                //filterLinear.Draw();
                //filterMix.Draw();
                waveText.Draw();

                Write("Tools", 441, 14, UIColors.label);
                bCopy.Draw();
                bPaste.Draw();
                bPhaseL.Draw();
                bPhaseR.Draw();
                bMoveUp.Draw();
                bMoveDown.Draw();
                bInvert.Draw();
                //bSmooth.Draw();
                bMutate.Draw();
                bNormalize.Draw();
                bModify.Draw();

                Color waveColor = new Color(200, 212, 93);
                Color waveBG = new Color(59, 125, 79, 150);
                // draw wave 
                if (displayAsLines) {
                    for (int i = 0; i < drawingRegion.width; ++i) {
                        int y1 = drawingRegion.y + drawingRegion.height / 2 - (int)(CurrentWave.GetSampleAtPosition(i / (float)drawingRegion.width) * drawingRegion.height / 2) - 3;
                        int y2 = drawingRegion.y + drawingRegion.height / 2 - (int)(CurrentWave.GetSampleAtPosition((i + 1) / (float)drawingRegion.width) * drawingRegion.height / 2) - 3;
                        if (y1 > y2) {
                            y1++;
                            y2++;
                        }
                        DrawRect(drawingRegion.x + i, y1, 1, -(y1 - drawingRegion.y) + drawingRegion.height / 2, waveBG);
                        DrawRect(drawingRegion.x + i, y1, 1, y1 == y2 ? 1 : y2 - y1, waveColor);
                    }
                }
                else {
                    for (int i = 0; i < 64; ++i) {
                        byte samp = CurrentWave.GetSample(i);

                        DrawRect(drawingRegion.x + i * 6, drawingRegion.y + drawingRegion.height / 2 + 1, 6, -5 * (samp - 16), waveBG);
                        DrawRect(drawingRegion.x + i * 6, drawingRegion.y + drawingRegion.height - samp * 5, 6, -5, waveColor);
                    }
                }
                // draw mini wave
                for (int i = 0; i < 64; ++i) {
                    DrawRect(419 + i, 183, 1, 16 - CurrentWave.GetSample(i + phase), new Color(190, 192, 211));
                    DrawRect(419 + i, 199 - CurrentWave.GetSample(i + phase), 1, 1, new Color(118, 124, 163));
                }


                if (IsMouseInCanvasBounds() && InFocus) {
                    DrawRect(drawingRegion.x + (drawingRegion.MouseX / 6 * 6), drawingRegion.y + drawingRegion.height - ((31 - drawingRegion.MouseY / 5) * 5), 6, -5, Helpers.Alpha(Color.White, 80));
                    if (Input.GetClick(KeyModifier.Shift)) {
                        int diff = Math.Abs(holdPosX - CanvasPosX);
                        if (diff > 0) {
                            if (holdPosX < CanvasPosX) {
                                for (int i = holdPosX; i <= CanvasPosX; ++i) {
                                    int y = (int)Math.Round(MathHelper.Lerp(holdPosY, CanvasPosY, (float)(i - holdPosX) / diff));
                                    DrawRect(drawingRegion.x + i * 6, drawingRegion.y + drawingRegion.height - y * 5, 6, -5, Helpers.Alpha(Color.White, 80));
                                }
                            }
                            else {
                                for (int i = CanvasPosX; i <= holdPosX; ++i) {
                                    int y = (int)Math.Round(MathHelper.Lerp(CanvasPosY, holdPosY, (float)(i - CanvasPosX) / diff));
                                    DrawRect(drawingRegion.x + i * 6, drawingRegion.y + drawingRegion.height - y * 5, 6, -5, Helpers.Alpha(Color.White, 80));
                                }
                            }
                        }
                    }
                }
                piano.Draw(App.pianoInput);
                Write("Resampling Filter", 413, 205 + 9, UIColors.label);
                ResampleDropdown.Draw();
                //MenuStrip.Draw();
            }
        }
    }
}
