using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveEditor : Window {
        public bool IsOpen { get { return WindowIsOpen || currentDialog != null && currentDialog.WindowIsOpen; } }
        public static bool enabled;
        public SpriteButton presetSine, presetTria, presetSaw, presetRect50, presetRect25, presetRect12, presetRand, presetClear;
        public Toggle filterNone, filterLinear, filterMix;
        public int startcooldown;
        public Button CopyButton, PasteButton, PhaseLButton, PhaseRButton, MoveUpButton, MoveDownButton, InvertButton, MutateButton, bSmooth, NormalizeButton;
        public DropdownButton ModifyButton;
        public Textbox waveText;
        public Dropdown resampleDropdown;
        public int id;
        private int holdPosY, holdPosX;
        private static string clipboardWave = "";
        private static ResamplingMode clipboardSampleMode = ResamplingMode.Mix;
        private PreviewPiano piano;
        private MouseRegion drawingRegion;
        private bool displayAsLines;
        private WaveModifyDialog currentDialog;

        private Wave CurrentWave {
            get {
                return App.CurrentModule.WaveBank[WaveBank.currentWaveID];
            }
        }

        private double phase;

        public WaveEditor() : base("Wave Editor", 500, 270) {

            int buttonY = 23;
            int buttonX = 420;
            int buttonWidth = 64;

            CopyButton = new Button("Copy", buttonX, buttonY, buttonWidth / 2 - 1, this);
            CopyButton.SetTooltip("", "Copy wave settings");
            buttonY += 0;
            PasteButton = new Button("Paste", buttonX + 32, buttonY, buttonWidth / 2 - 1, this);
            PasteButton.SetTooltip("", "Paste wave settings");
            buttonY += 18;

            PhaseRButton = new Button("Phase →", buttonX, buttonY, buttonWidth, this);
            PhaseRButton.SetTooltip("", "Shift phase once to the right");
            buttonY += 14;
            PhaseLButton = new Button("Phase ←", buttonX, buttonY, buttonWidth, this);
            PhaseLButton.SetTooltip("", "Shift phase once to the left");
            buttonY += 14;
            MoveUpButton = new Button("Shift Up", buttonX, buttonY, buttonWidth, this);
            MoveUpButton.SetTooltip("", "Raise the wave 1 step up");
            buttonY += 14;
            MoveDownButton = new Button("Shift Down", buttonX, buttonY, buttonWidth, this);
            MoveDownButton.SetTooltip("", "Lower the wave 1 step down");
            buttonY += 18;

            InvertButton = new Button("Invert", buttonX, buttonY, buttonWidth, this);
            InvertButton.SetTooltip("", "Invert the wave vertically");
            buttonY += 14;

            MutateButton = new Button("Mutate", buttonX, buttonY, buttonWidth, this);
            MutateButton.SetTooltip("", "Slightly randomize the wave");
            buttonY += 14;
            NormalizeButton = new Button("Normalize", buttonX, buttonY, buttonWidth, this);
            NormalizeButton.SetTooltip("", "Make the wave maximum amplitude");
            buttonY += 18;

            ModifyButton = new DropdownButton("Modify...", buttonX, buttonY, buttonWidth, this);
            ModifyButton.LabelIsCentered = true;
            ModifyButton.SetMenuItems(["Smooth...", "Add Fuzz...", "Sync...", "Sample and hold..."]);

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

            ExitButton.SetTooltip("Close", "Close wave editor");

            drawingRegion = new MouseRegion(17, 23, 384, 160, this);

            waveText = new Textbox("", 17, 188, 384, 384, this);
            waveText.canEdit = true;
            waveText.InputField.MaximumLength = 192;

            resampleDropdown = new Dropdown(385, 215, this);
            resampleDropdown.SetMenuItems(["Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)"]);
            piano = new PreviewPiano(10, 240, this);

        }
        public void Open(int waveIndex) {
            base.Open();
            startcooldown = 10;
            id = waveIndex;
            enabled = true;
        }

        public new void Close() {
            currentDialog = null;
            enabled = false;
            base.Close();

        }

        public int GetPianoMouseInput() {
            return !enabled || !InFocus ? -1 : piano.CurrentClickedNote;
        }

        private bool IsMouseInCanvasBounds() {
            return InFocus && drawingRegion.IsHovered;
        }

        private int CanvasPosX {
            get {
                return drawingRegion.MouseX / 6;
            }
        }

        private int CanvasPosY {
            get {
                return 31 - drawingRegion.MouseY / 5;
            }
        }

        private void ToggleDisplayMode() {
            displayAsLines = !displayAsLines;
        }

        public void Update() {
            if (WindowIsOpen && !(currentDialog != null && currentDialog.WindowIsOpen)) {
                DoDragging();
                if (WaveBank.currentWaveID < 0) {
                    return;
                }

                if (WaveBank.currentWaveID > 99) {
                    return;
                }

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
                    ChannelManager.PreviewChannel.SetWave(WaveBank.currentWaveID);
                }

                //Temp fix for PreviewChannel wave being changed when song is played
                if(ChannelManager.PreviewChannel.WaveIndex != WaveBank.currentWaveID) {
                    ChannelManager.PreviewChannel.SetWave(WaveBank.currentWaveID);
                }

                resampleDropdown.Value = (int)CurrentWave.resamplingMode;
                if (startcooldown > 0) {
                    waveText.Text = CurrentWave.ToNumberString();
                    startcooldown--;
                }
                else {
                    if (InFocus && (ExitButton.Clicked || Input.GetKeyDown(Keys.Escape, KeyModifier.None))) {
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

                    resampleDropdown.Update();
                    if (resampleDropdown.ValueWasChangedInternally) {
                        App.CurrentModule.SetDirty();
                    }
                    CurrentWave.resamplingMode = (ResamplingMode)resampleDropdown.Value;

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
                    if (CopyButton.Clicked) {
                        clipboardWave = CurrentWave.ToString();
                        clipboardSampleMode = CurrentWave.resamplingMode;
                    }
                    if (PasteButton.Clicked) {
                        if (clipboardWave.Length == 64) {
                            CurrentWave.SetWaveformFromString(clipboardWave);
                            CurrentWave.resamplingMode = clipboardSampleMode;
                            App.CurrentModule.SetDirty();
                        }
                    }

                    if (PhaseLButton.Clicked) {
                        CurrentWave.ShiftPhase(1);
                        App.CurrentModule.SetDirty();
                    }
                    if (PhaseRButton.Clicked) {
                        CurrentWave.ShiftPhase(-1);
                        App.CurrentModule.SetDirty();
                    }
                    if (MoveUpButton.Clicked) {
                        CurrentWave.Move(1);
                        App.CurrentModule.SetDirty();
                    }
                    if (MoveDownButton.Clicked) {
                        CurrentWave.Move(-1);
                        App.CurrentModule.SetDirty();
                    }
                    if (InvertButton.Clicked) {
                        CurrentWave.Invert();
                        App.CurrentModule.SetDirty();
                    }

                    if (MutateButton.Clicked) {
                        CurrentWave.MutateHarmonics();
                        App.CurrentModule.SetDirty();
                    }
                    if (NormalizeButton.Clicked) {
                        CurrentWave.Normalize();
                        App.CurrentModule.SetDirty();
                    }
                    ModifyButton.Update();
                    if (ModifyButton.SelectedAnItem) {
                        switch (ModifyButton.SelectedIndex) {
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
                                [
                                    new MenuOption("Show as grid",ToggleDisplayMode,displayAsLines),
                                    new MenuOption("Show as line",ToggleDisplayMode,!displayAsLines),
                                ]
                            ));
                        }
                        if (drawingRegion.ClickedDownM(KeyModifier._Any)) {
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
            if (WindowIsOpen) {
                name = "Edit Wave " + id.ToString("D2");
                base.Draw();

                Write("Draw Wave", 17, 14, UIColors.label);
                DrawSprite(16, 22, new Rectangle(0, 144, 386, 162));

                Write("Presets", 17, 205, UIColors.label);
                presetSine.Draw();
                presetTria.Draw();
                presetRect50.Draw();
                presetSaw.Draw();
                presetRect25.Draw();
                presetRect12.Draw();
                presetClear.Draw();
                presetRand.Draw();

                waveText.Draw();

                Write("Tools", 441, 14, UIColors.label);
                CopyButton.Draw();
                PasteButton.Draw();
                PhaseLButton.Draw();
                PhaseRButton.Draw();
                MoveUpButton.Draw();
                MoveDownButton.Draw();
                InvertButton.Draw();
                MutateButton.Draw();
                NormalizeButton.Draw();
                ModifyButton.Draw();

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

                        DrawRect(drawingRegion.x + i * 6, drawingRegion.y + drawingRegion.height / 2, 6, -5 * (samp - 16), waveBG);
                        DrawRect(drawingRegion.x + i * 6, drawingRegion.y + drawingRegion.height - samp * 5, 6, -5, waveColor);
                    }
                }
                phase += App.GameTime.ElapsedGameTime.TotalMilliseconds / 16f;
                if (phase > 128) {
                    phase -= 64;
                }
                // draw mini wave
                for (int i = 0; i < 64; ++i) {
                    DrawRect(419 + i, 185, 1, 16 - CurrentWave.GetSample(i + (int)phase), new Color(190, 192, 211));
                    DrawRect(419 + i, 201 - CurrentWave.GetSample(i + (int)phase), 1, 1, new Color(118, 124, 163));
                }

                if (IsMouseInCanvasBounds() && InFocus) {
                    DrawRect(drawingRegion.x + drawingRegion.MouseX / 6 * 6, drawingRegion.y + drawingRegion.height - (31 - drawingRegion.MouseY / 5) * 5, 6, -5, Helpers.Alpha(Color.White, 80));
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
                piano.Draw(PianoInput.CurrentNote);
                WriteRightAlign("Resampling Mode", resampleDropdown.x - 4, resampleDropdown.y + 4, UIColors.label);
                resampleDropdown.Draw();
            }
        }
    }
}
