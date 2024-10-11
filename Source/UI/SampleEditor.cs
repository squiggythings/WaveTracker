using Microsoft.Xna.Framework;
using System;
using WaveTracker.Audio;
using WaveTracker.Tracker;
using System.Collections.Generic;
using SharpDX.DirectWrite;

namespace WaveTracker.UI {
    public class SampleEditor : Element {
        /// <summary>
        /// The sample to edit
        /// </summary>
        public Sample Sample { get; set; }

        private MouseRegion waveformRegion;

        private int MouseSampleIndex { get { return (int)(waveformRegion.MouseXPercentage * ViewportSize + 0.5f) + ViewportOffset; } }

        private int MouseSampleIndexClamped { get { return Math.Clamp((int)(waveformRegion.MouseXPercentage * ViewportSize + 0.5f) + ViewportOffset, 0, Sample.Length); } }
        private int selectionStartIndex;
        private int selectionEndIndex;

        private List<SampleEditorState> history;
        private int historyIndex;

        float zoomLevel = 1;
        int ViewportSize => (int)(Sample.Length / zoomLevel);
        int ViewportOffset { get; set; }

        private bool SelectionIsActive { get; set; }

        private int SelectionMin {
            get {
                return Math.Min(selectionStartIndex, selectionEndIndex);
            }
        }

        private int SelectionMax {
            get {
                return Math.Max(selectionStartIndex, selectionEndIndex);
            }
        }

        private NumberBox baseKey;
        private NumberBox fineTune;
        private Button importSample;
        private Dropdown resamplingMode;
        private Dropdown loopMode;
        private NumberBox loopPoint;
        private SampleBrowser browser;
        private Button normalize, reverse, fadeIn, fadeOut, amplifyUp, amplifyDown, invert, cut;
        private CheckboxLabeled showInVisualizer;
        private int lastMouseHoverSample;
        private ScrollbarHorizontal viewportScrollbar;


        public SampleEditor(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            SetParent(parent);
            history = new List<SampleEditorState>();

            waveformRegion = new MouseRegion(0, 10, 568, 175 - 4, this);


            importSample = new Button("Import Sample    ", 0, 188, this);
            importSample.SetTooltip("", "Import an audio file into the instrument");

            resamplingMode = new Dropdown(242, 237, this);
            resamplingMode.SetMenuItems(["Harsh (None)", "Smooth (Linear)", "Mix (None + Linear)"]);

            baseKey = new NumberBox("Base Key", 0, 220, 100, 56, this);
            baseKey.SetValueLimits(12, 131);
            baseKey.DisplayMode = NumberBox.NumberDisplayMode.Note;
            baseKey.SetTooltip("", "The note where the sample is played at its original speed");

            fineTune = new NumberBox("Fine tune", 0, 237, 100, 56, this);
            fineTune.SetValueLimits(-199, 199);
            fineTune.DisplayMode = NumberBox.NumberDisplayMode.PlusMinus;
            fineTune.SetTooltip("", "Slightly adjust the pitch of the sample, in cents");

            loopPoint = new NumberBox("Loop position (samples)", 154, 203, 186, 83, this);
            loopPoint.SetTooltip("", "Set the position in audio samples where the sound loops back to");

            loopMode = new Dropdown(282, 188, this, false);
            loopMode.SetMenuItems(["One-shot", "Forward", "Ping-pong"]);

            int buttonsX = 357;
            int buttonsY = 188;
            int buttonsWidth = 50;
            normalize = new Button("Normalize", buttonsX, buttonsY, buttonsWidth, this);
            buttonsX += buttonsWidth + 1;
            fadeIn = new Button("Fade in", buttonsX, buttonsY, buttonsWidth, this);
            buttonsX += buttonsWidth + 1;
            amplifyUp = new Button("Amplify+", buttonsX, buttonsY, buttonsWidth, this);
            buttonsX += buttonsWidth + 1;
            invert = new Button("Invert", buttonsX, buttonsY, buttonsWidth, this);
            buttonsX = 357;
            buttonsY += 14;
            reverse = new Button("Reverse", buttonsX, buttonsY, buttonsWidth, this);
            buttonsX += buttonsWidth + 1;
            fadeOut = new Button("Fade out", buttonsX, buttonsY, buttonsWidth, this);
            buttonsX += buttonsWidth + 1;
            amplifyDown = new Button("Amplify-", buttonsX, buttonsY, buttonsWidth, this);
            buttonsX += buttonsWidth + 1;
            cut = new Button("Cut", buttonsX, buttonsY, buttonsWidth, this);

            showInVisualizer = new CheckboxLabeled("Show in visualizer", 480, 245, 88, this);
            showInVisualizer.ShowCheckboxOnRight = true;
            browser = new SampleBrowser();
            viewportScrollbar = new ScrollbarHorizontal(waveformRegion.x, waveformRegion.BoundsBottom, waveformRegion.width, 6, this);
        }

        public void Reset() {
            zoomLevel = 1;
            ViewportOffset = 0;
            selectionStartIndex = 0;
            selectionEndIndex = 0;
            SelectionIsActive = false;
            ClearHistory();
        }

        public void Update() {
            if (InFocus) {
                lastMouseHoverSample = MouseSampleIndexClamped;
            }
            if (!browser.InFocus && Input.focusTimer > 2) {
                if (Sample.Length > 0) {
                    if (waveformRegion.DidClickInRegionM(KeyModifier.None)) {
                        if (waveformRegion.ClickedDown) {
                            selectionStartIndex = MouseSampleIndexClamped;
                        }
                        if (waveformRegion.MouseXPercentage < 0) {
                            ViewportOffset -= Math.Max(1, (int)(Sample.Length / 100 / zoomLevel));
                        }
                        if (waveformRegion.MouseXPercentage > 1f) {
                            ViewportOffset += Math.Max(1, (int)(Sample.Length / 100 / zoomLevel));
                        }
                        ViewportOffset = Math.Clamp(ViewportOffset, 0, Sample.Length - ViewportSize);
                        selectionEndIndex = MouseSampleIndexClamped;
                        SelectionIsActive = selectionStartIndex != selectionEndIndex;
                    }
                    if (waveformRegion.DidClickInRegionM(KeyModifier.Shift)) {
                        if (waveformRegion.MouseXPercentage < 0) {
                            ViewportOffset -= Math.Max(1, (int)(Sample.Length / 100 / zoomLevel));
                        }
                        if (waveformRegion.MouseXPercentage > 1f) {
                            ViewportOffset += Math.Max(1, (int)(Sample.Length / 100 / zoomLevel));
                        }
                        if (MouseSampleIndexClamped - (selectionStartIndex + selectionEndIndex) / 2 < 0) {
                            if (selectionStartIndex < selectionEndIndex) {
                                selectionStartIndex = MouseSampleIndexClamped;
                            }
                            else {
                                selectionEndIndex = MouseSampleIndexClamped;
                            }
                        }
                        else {
                            if (selectionStartIndex > selectionEndIndex) {
                                selectionStartIndex = MouseSampleIndexClamped;
                            }
                            else {
                                selectionEndIndex = MouseSampleIndexClamped;
                            }
                        }

                    }
                    if (waveformRegion.Clicked) {
                        if (selectionStartIndex == selectionEndIndex) {
                            SelectionIsActive = false;
                        }
                    }
                    if (waveformRegion.RightClicked) {
                        ContextMenu.Open(new Menu(new MenuItemBase[] {
                            new MenuOption("Select all", SelectAll),
                            new MenuOption("Deselect", Deselect, SelectionIsActive),
                            null,
                            new MenuOption("Cut", Cut, SelectionIsActive),
                            null,
                            new MenuOption("Set loop point", SetLoopPoint),
                            null,
                            new SubMenu("Tools", new MenuItemBase[] {
                                new MenuOption("Reverse", Reverse),
                                new MenuOption("Normalize", Normalize),
                                new MenuOption("Invert", Invert),
                                new MenuOption("Fade In", FadeIn),
                                new MenuOption("Fade Out", FadeOut),
                                new MenuOption("Amplify+", AmplifyUp),
                                new MenuOption("Amplify-", AmplifyDown),
                            }),
                            null,
                            new MenuOption("Export...", Sample.SaveToDisk),
                        }));
                    }
                }
                if (waveformRegion.DidClickInRegionM(KeyModifier.Alt)) {
                    SetLoopPoint();
                }

                if (importSample.Clicked) {
                    browser.Open(this);
                    Reset();
                }

                baseKey.Value = Sample.BaseKey;
                baseKey.Update();
                if (baseKey.ValueWasChangedInternally) {
                    Sample.SetBaseKey(baseKey.Value);
                    App.CurrentModule.SetDirty();
                }
                fineTune.Value = Sample.Detune;
                fineTune.Update();
                if (fineTune.ValueWasChangedInternally) {
                    Sample.SetDetune(fineTune.Value);
                    App.CurrentModule.SetDirty();
                }
                resamplingMode.Value = (int)Sample.resampleMode;
                resamplingMode.Update();
                if (resamplingMode.ValueWasChangedInternally) {
                    Sample.resampleMode = (ResamplingMode)resamplingMode.Value;
                    App.CurrentModule.SetDirty();
                }
                loopMode.Value = (int)Sample.loopType;
                loopMode.Update();
                if (loopMode.ValueWasChangedInternally) {
                    Sample.loopType = (Sample.LoopType)loopMode.Value;
                    AddToUndoHistory();
                }
                loopPoint.enabled = Sample.loopType != Sample.LoopType.OneShot;
                loopPoint.SetValueLimits(0, Math.Max(0, Sample.Length - 1));
                loopPoint.Value = Sample.loopPoint;
                loopPoint.Update();
                if (loopPoint.ValueWasChangedInternally) {
                    Sample.loopPoint = loopPoint.Value;
                    AddToUndoHistory();
                }

                if (normalize.Clicked) {
                    Normalize();
                }
                if (fadeIn.Clicked) {
                    FadeIn();
                }
                if (fadeOut.Clicked) {
                    FadeOut();
                }
                if (amplifyUp.Clicked) {
                    AmplifyUp();
                }
                if (amplifyDown.Clicked) {
                    AmplifyDown();
                }
                if (invert.Clicked) {
                    Invert();
                }
                if (reverse.Clicked) {
                    Reverse();
                }
                cut.enabled = SelectionIsActive;
                if (cut.Clicked) {
                    Cut();
                }

                if (App.Shortcuts["Edit\\Undo"].IsPressedDown) {
                    Undo();
                }
                if (App.Shortcuts["Edit\\Redo"].IsPressedDown) {
                    Redo();
                }
                if (waveformRegion.IsHovered) {
                    float oldZoomlevel = zoomLevel;
                    int oldMouse = MouseSampleIndex;
                    if (Input.MouseScrollWheel(KeyModifier.Ctrl) > 0) {
                        zoomLevel *= 1.1f;
                    }
                    else if (Input.MouseScrollWheel(KeyModifier.Ctrl) < 0) {
                        zoomLevel /= 1.1f;
                    }
                    if (zoomLevel < 1f) {
                        zoomLevel = 1f;
                    }
                    if (zoomLevel > Math.Max(Sample.Length / (float)waveformRegion.width * 25, 1)) {
                        zoomLevel = Math.Max(Sample.Length / (float)waveformRegion.width * 25, 1);
                    }
                    float delta = zoomLevel - oldZoomlevel;
                    //ViewportOffset += (int)(delta * waveformRegion.MouseXClamped);
                    ViewportOffset -= MouseSampleIndex - oldMouse;
                    // panning
                    ViewportOffset -= (int)(Input.MouseScrollWheel(KeyModifier.Shift) * (Sample.Length / 100 / zoomLevel));
                }
                if (zoomLevel < 1f) {
                    zoomLevel = 1f;
                }
                if (zoomLevel > Math.Max(Sample.Length / (float)waveformRegion.width * 25, 1)) {
                    zoomLevel = Math.Max(Sample.Length / (float)waveformRegion.width * 25, 1);
                }
                viewportScrollbar.SetSize(Sample.Length, ViewportSize);
                viewportScrollbar.CoarseStepAmount = (int)(Sample.Length / 100 / zoomLevel);
                viewportScrollbar.ScrollValue = ViewportOffset;
                viewportScrollbar.Update();
                ViewportOffset = Math.Clamp(viewportScrollbar.ScrollValue, 0, Sample.Length - ViewportSize);

                showInVisualizer.Value = Sample.useInVisualization;
                showInVisualizer.Update();
                Sample.useInVisualization = showInVisualizer.Value;
            }
            browser.Update();
        }

        private void Cut() {
            Sample.Cut(SelectionMin, SelectionMax);
            SelectionIsActive = false;
            AddToUndoHistory();
        }

        private void Normalize() {
            if (SelectionIsActive) {
                Sample.Normalize(SelectionMin, SelectionMax);
            }
            else {
                Sample.Normalize();
            }
            AddToUndoHistory();
        }

        private void FadeIn() {
            if (SelectionIsActive) {
                Sample.FadeIn(SelectionMin, SelectionMax);
            }
            else {
                Sample.FadeIn();
            }
            AddToUndoHistory();
        }

        private void FadeOut() {
            if (SelectionIsActive) {
                Sample.FadeOut(SelectionMin, SelectionMax);
            }
            else {
                Sample.FadeOut();
            }
            AddToUndoHistory();
        }

        private void Invert() {
            if (SelectionIsActive) {
                Sample.Invert(SelectionMin, SelectionMax);
            }
            else {
                Sample.Invert();
            }
            AddToUndoHistory();
        }

        private void AmplifyUp() {
            if (SelectionIsActive) {
                Sample.Amplify(1.1f, SelectionMin, SelectionMax);
            }
            else {
                Sample.Amplify(1.1f);
            }
            AddToUndoHistory();
        }

        private void AmplifyDown() {
            if (SelectionIsActive) {
                Sample.Amplify(0.9090909f, SelectionMin, SelectionMax);
            }
            else {
                Sample.Amplify(0.9090909f);
            }
            AddToUndoHistory();
        }

        private void Reverse() {
            if (SelectionIsActive) {
                Sample.Reverse(SelectionMin, SelectionMax);
            }
            else {
                Sample.Reverse();
            }
            AddToUndoHistory();
        }

        private void Deselect() {
            SelectionIsActive = false;
        }

        private void SelectAll() {
            SelectionIsActive = true;
            selectionStartIndex = 0;
            selectionEndIndex = Sample.Length - 1;
        }

        private void SetLoopPoint() {
            if (SelectionIsActive) {
                Sample.loopPoint = Math.Clamp(SelectionMin, 0, Math.Max(0, Sample.Length - 1));
            }
            else {
                Sample.loopPoint = Math.Clamp(lastMouseHoverSample, 0, Math.Max(0, Sample.Length - 1));
            }

            if (Sample.loopType == Sample.LoopType.OneShot) {
                Sample.loopType = Sample.LoopType.Forward;
            }
            AddToUndoHistory();
            App.CurrentModule.SetDirty();
        }

        public void Draw() {
            if (Sample == null) {
                return;
            }
            string name;
            if (Sample.name == null || Sample.name.Length == 0) {
                name = "";
            }
            else {
                name = Sample.name + "  ";
            }
            Write(name + "(" + Sample.Length + " samples)", waveformRegion.x, waveformRegion.y - 9, UIColors.label);
            WriteRightAlign((Sample.Length / (float)AudioEngine.SampleRate).ToString("F5") + " seconds", waveformRegion.x + waveformRegion.width, waveformRegion.y - 9, UIColors.label);

            if (Sample.IsStereo && Sample.Length > 0) {
                DrawWaveform(waveformRegion.x, waveformRegion.y, Sample.sampleDataL, waveformRegion.width, waveformRegion.height / 2);
                DrawWaveform(waveformRegion.x, waveformRegion.y + waveformRegion.height / 2 + 1, Sample.sampleDataR, waveformRegion.width, waveformRegion.height / 2);
            }
            else {
                DrawWaveform(waveformRegion.x, waveformRegion.y, Sample.sampleDataL, waveformRegion.width, waveformRegion.height);
            }
            viewportScrollbar.Draw();
            importSample.Draw();
            DrawSprite(importSample.x + importSample.width - 14, importSample.y + (importSample.IsPressed ? 3 : 2), new Rectangle(72, 81, 12, 9));
            loopMode.Draw();
            WriteRightAlign("Loop Mode", loopMode.x - 4, loopMode.y + 4, UIColors.label);
            if (Sample.loopType != Sample.LoopType.OneShot) {
                loopPoint.Draw();
            }
            baseKey.Draw();
            fineTune.Draw();
            resamplingMode.Draw();
            WriteRightAlign("Resampling Mode", resamplingMode.x - 4, resamplingMode.y + 4, UIColors.label);
            loopMode.Draw();
            DrawRect(348, 188, 1, 66, UIColors.label);
            normalize.Draw();
            invert.Draw();
            cut.Draw();
            fadeIn.Draw();
            fadeOut.Draw();
            amplifyDown.Draw();
            amplifyUp.Draw();
            reverse.Draw();
            showInVisualizer.Draw();
            browser.Draw();
        }

        private void DrawWaveform(int x, int y, short[] data, int width, int height) {
            int startY = y + height / 2;
            uint nextSampleIndex;
            uint sampleIndex;
            Color sampleColor = new Color(207, 117, 43);
            DrawRect(x, y, width, height, UIColors.black);
            if (data.Length > 0) {
                DrawRect(x, startY, width, 1, UIColors.labelDark);
                if (SelectionIsActive) {
                    int x1 = GetXPositionOfSample(SelectionMin, width);
                    int x2 = GetXPositionOfSample(SelectionMax, width);
                    DrawRect(x + x1, y, x2 - x1, height, Helpers.Alpha(UIColors.selection, 128));
                }
                int loopPosition = GetXPositionOfSample(Sample.loopPoint, width);
                if (Sample.loopType != Sample.LoopType.OneShot) {
                    DrawRect(x + loopPosition, y, width - loopPosition, height, Helpers.Alpha(Color.Yellow, 50));
                }

                for (int wx = 0; wx < width; wx++) {
                    sampleIndex = (uint)(ViewportOffset + wx / (double)width * ViewportSize);
                    nextSampleIndex = (uint)(ViewportOffset + (wx + 1) / (double)width * ViewportSize);
                    if (nextSampleIndex >= data.Length) {
                        nextSampleIndex = sampleIndex;
                    }
                    float min = 1;
                    float max = -1;
                    float average = 0;
                    uint underflowSkip = (uint)((nextSampleIndex - sampleIndex) / (width * 2));
                    for (uint j = sampleIndex; j <= nextSampleIndex; ++j) {
                        float value;
                        if (j >= 0 && j < data.Length) {
                            value = data[j] / (float)short.MaxValue;
                        }
                        else {
                            value = 0;
                        }
                        average += MathF.Abs(value);
                        if (value < min) {
                            min = value;
                        }

                        if (value > max) {
                            max = value;
                        }

                        if (underflowSkip > 0) {
                            j += underflowSkip;
                            average += MathF.Abs(value) * underflowSkip;
                        }
                    }
                    average /= nextSampleIndex - sampleIndex + 1;
                    int rectStart = (int)(max * height / 2);
                    int rectEnd = (int)(min * height / 2);
                    int avgStart = (int)(average * height / -2);
                    int avgEnd = (int)(average * height / 2);
                    if (SelectionIsActive && sampleIndex >= SelectionMin && nextSampleIndex < SelectionMax) {
                        DrawRect(x + wx, startY - rectStart, 1, rectStart - rectEnd + 1, Color.LightGray);
                    }
                    else {
                        DrawRect(x + wx, startY - rectStart, 1, rectStart - rectEnd + 1, sampleColor);
                    }
                    avgStart = Math.Clamp(avgStart, rectEnd, rectStart);
                    avgEnd = Math.Clamp(avgEnd, rectEnd, rectStart);
                    DrawRect(x + wx, startY - avgStart, 1, avgStart - avgEnd + 1, Helpers.Alpha(Color.White, 90));

                }


                if (Sample.loopType != Sample.LoopType.OneShot) {
                    DrawRect(x + loopPosition, y, 1, height, Color.Yellow);
                }
                if (Sample.currentPlaybackPosition < data.Length && ChannelManager.PreviewChannel.IsPlaying) {
                    DrawRect(x + GetXPositionOfSample(Sample.currentPlaybackPosition, width), y, 1, height, Color.Aqua);
                }
                if (waveformRegion.IsHovered) {
                    DrawRect(x + GetXPositionOfSample(MouseSampleIndex, width), y, 1, height, Helpers.Alpha(Color.White, 128));
                }
                else if (!InFocus && !browser.InFocus && !SelectionIsActive) {
                    DrawRect(x + GetXPositionOfSample(lastMouseHoverSample, width), y, 1, height, Helpers.Alpha(Color.White, 64));
                }
            }
        }

        private int GetXPositionOfSample(int sampleIndex, int windowWidth) {
            return (int)Math.Clamp((float)(sampleIndex - ViewportOffset) / (ViewportSize) * windowWidth, 0, windowWidth);
        }

        public void ClearHistory() {
            history.Clear();
            history.Add(new SampleEditorState(Sample));
            historyIndex = 0;
        }

        public void AddToUndoHistory() {
            // initialize 
            if (history.Count == 0) {
                history.Add(new SampleEditorState(Sample));
                historyIndex = 0;
                return;
            }

            while (history.Count - 1 > historyIndex) {
                history.RemoveAt(history.Count - 1);
            }
            history.Add(new SampleEditorState(Sample));
            historyIndex++;
            if (history.Count > 64) {
                history.RemoveAt(0);
                historyIndex--;
            }
            App.CurrentModule.SetDirty();
        }

        public void Undo() {
            // set selection

            historyIndex--;
            if (historyIndex < 0) {
                historyIndex = 0;
            }

            history[historyIndex].RestoreIntoSample(Sample);

            App.CurrentModule.SetDirty();
        }
        public void Redo() {

            historyIndex++;
            if (historyIndex >= history.Count) {
                historyIndex = history.Count - 1;
            }

            history[historyIndex].RestoreIntoSample(Sample);

            // set selection

            App.CurrentModule.SetDirty();
        }




        public record SampleEditorState {
            short[] channelLeft;
            short[] channelRight;
            Sample.LoopType loopType;
            int loopPoint;

            public SampleEditorState(Sample sample) {
                channelLeft = new short[sample.sampleDataL.Length];
                Array.Copy(sample.sampleDataL, channelLeft, sample.sampleDataL.Length);
                channelRight = new short[sample.sampleDataR.Length];
                Array.Copy(sample.sampleDataR, channelRight, sample.sampleDataR.Length);
                loopPoint = sample.loopPoint;
                loopType = sample.loopType;
            }

            public void RestoreIntoSample(Sample sample) {
                sample.sampleDataL = new short[channelLeft.Length];
                Array.Copy(channelLeft, sample.sampleDataL, channelLeft.Length);
                sample.sampleDataR = new short[channelRight.Length];
                Array.Copy(channelRight, sample.sampleDataR, channelRight.Length);
                sample.loopPoint = loopPoint;
                sample.loopType = loopType;
            }
        }
    }

}
