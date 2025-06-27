﻿using Microsoft.Xna.Framework;
using System;
using WaveTracker.Audio;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class EnvelopeEditor : Clickable {
        private Envelope currentEnvelope;
        private Scrollbar scrollbar;

        private int PlaybackStep { get { return playbackEnvelopePlayer.step; } }

        private bool IsPlaying { get { return !playbackEnvelopePlayer.EnvelopeEnded; } }

        private EnvelopePlayer playbackEnvelopePlayer;
        private Textbox envText;
        private int arpRange = 120;
        private int arpHeight = 6;
        private int waveRange = 101;
        private int waveHeight = 6;
        private NumberBox envLength;
        public int lastEnvType;
        private int holdPosX, holdPosY;
        private MouseRegion drawingRegion;
        private const int MARGIN_WIDTH = 40;

        protected new bool InFocus => base.InFocus || envLength.InFocus || envText.InFocus;

        //445
        //364
        private int ColumnWidth {
            get {
                return currentEnvelope.Length == 0 ? 0 : Math.Clamp((width - MARGIN_WIDTH - 14) / currentEnvelope.Length, 1, 48);
            }
        }

        public EnvelopeEditor(int x, int y, int width, Element parent) {
            this.x = x;
            this.y = y;
            this.width = width;
            height = 222;
            drawingRegion = new MouseRegion(MARGIN_WIDTH + 3, 22, 482, 200, this);
            scrollbar = new Scrollbar(MARGIN_WIDTH, 20, this.width - MARGIN_WIDTH - 1, 200, this);
            scrollbar.CoarseStepAmount = 2;

            envLength = new NumberBox("Length", 0, 224, 74, 38, this);
            envLength.SetValueLimits(0, Envelope.MAX_ENVELOPE_LENGTH);

            envText = new Textbox("", 0, 240, this.width, this);
            envText.InputField.MaximumLength = 800;

            SetParent(parent);
            ResetScrollbar();

        }

        public void ResetScrollbar() {
            scrollbar.ScrollValue = arpRange - 16;
            scrollbar.Update();
        }

        private int GetXPositionOfColumn(int index) {
            return MARGIN_WIDTH + 3 + index * ColumnWidth;
        }

        private int GetYPositionOfValue(int value) {
            return currentEnvelope.Type switch {
                Envelope.EnvelopeType.Volume or Envelope.EnvelopeType.WaveBlend or Envelope.EnvelopeType.WaveStretch or Envelope.EnvelopeType.WaveFM or Envelope.EnvelopeType.WaveSync => drawingRegion.y + (99 - value) * 2,
                Envelope.EnvelopeType.Arpeggio => drawingRegion.y + (arpRange - value - scrollbar.ScrollValue) * arpHeight,
                Envelope.EnvelopeType.Pitch => drawingRegion.y + (99 - value),
                Envelope.EnvelopeType.Wave => drawingRegion.y + (waveRange - value - scrollbar.ScrollValue - 2) * waveHeight,
                _ => 0,
            };
        }

        public void Update() {

            if (enabled && InFocus) {
                envLength.enabled = currentEnvelope.IsActive;
                envText.enabled = currentEnvelope.IsActive;

                envLength.Value = currentEnvelope.Length;
                envLength.Update();
                if (envLength.ValueWasChangedInternally) {
                    currentEnvelope.Resize(envLength.Value);
                    App.CurrentModule.SetDirty();
                }

                if (currentEnvelope.IsActive) {
                    scrollbar.Update();
                }

                envText.Text = currentEnvelope.ToString();
                envText.Update();
                if (envText.ValueWasChangedInternally) {
                    currentEnvelope.LoadFromString(envText.Text);
                    App.CurrentModule.SetDirty();
                }
                envText.Text = currentEnvelope.ToString();
                int canvasPosX = CanvasMouseBlockClamped().X;
                int canvasPosY = CanvasMouseBlockClamped().Y;
                if (PointIsInCanvas(LastClickPos)) {

                    if (Input.GetClickDown(KeyModifier._Any)) {
                        holdPosX = CanvasMouseBlockClamped().X;
                        holdPosY = CanvasMouseBlockClamped().Y;
                    }
                    if (Input.GetClick(KeyModifier.None)) {
                        currentEnvelope.values[Math.Clamp(canvasPosX, 0, currentEnvelope.Length - 1)] = (sbyte)canvasPosY;
                        App.CurrentModule.SetDirty();
                    }

                    if (Input.GetClickUp(KeyModifier.Shift)) {
                        int diff = Math.Abs(holdPosX - canvasPosX);
                        if (diff > 0) {
                            if (holdPosX < canvasPosX) {
                                for (int i = holdPosX; i <= canvasPosX; ++i) {
                                    currentEnvelope.values[i] = (sbyte)Math.Round(Lerp(holdPosY, canvasPosY, (float)(i - holdPosX) / diff));
                                }
                            }
                            else {
                                for (int i = canvasPosX; i <= holdPosX; ++i) {
                                    currentEnvelope.values[i] = (sbyte)Math.Round(Lerp(canvasPosY, holdPosY, (float)(i - canvasPosX) / diff));
                                }
                            }
                        }
                        else {
                            currentEnvelope.values[Math.Clamp(canvasPosX, 0, currentEnvelope.Length - 1)] = (sbyte)canvasPosY;
                        }
                        App.CurrentModule.SetDirty();
                    }

                }

                // input loop/release
                #region loop/release
                if (Input.GetClickDown(KeyModifier.None) && currentEnvelope.IsActive) {
                    if (MouseIsInReleaseRibbon && MouseEnvelopeX >= 1 && MouseEnvelopeX < currentEnvelope.Length) {
                        currentEnvelope.ReleaseIndex = currentEnvelope.ReleaseIndex != MouseEnvelopeX - 1 ? (byte)(MouseEnvelopeX - 1) : (byte)Envelope.EMPTY_LOOP_RELEASE_INDEX;
                        App.CurrentModule.SetDirty();
                    }
                    if (MouseIsInLoopRibbon && MouseEnvelopeX >= 0 && MouseEnvelopeX < currentEnvelope.Length) {
                        currentEnvelope.LoopIndex = currentEnvelope.LoopIndex != MouseEnvelopeX ? (byte)MouseEnvelopeX : (byte)Envelope.EMPTY_LOOP_RELEASE_INDEX;
                        App.CurrentModule.SetDirty();
                    }
                }
                #endregion
            }
        }

        private static float Lerp(float firstFloat, float secondFloat, float by) {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public Point CanvasMouseBlock() {
            int x = (int)(drawingRegion.MouseX / (float)ColumnWidth);
            int mY = drawingRegion.MouseY;
            int y = 0;
            switch (currentEnvelope.Type) {
                case Envelope.EnvelopeType.Volume:
                case Envelope.EnvelopeType.WaveBlend:
                case Envelope.EnvelopeType.WaveStretch:
                case Envelope.EnvelopeType.WaveFM:
                case Envelope.EnvelopeType.WaveSync:
                    y = 99 - mY / 2;
                    break;
                case Envelope.EnvelopeType.Arpeggio:
                    y = arpRange - mY / arpHeight - scrollbar.ScrollValue;
                    break;
                case Envelope.EnvelopeType.Pitch:
                    y = 99 - mY;
                    break;
                case Envelope.EnvelopeType.Wave:
                    y = waveRange - mY / waveHeight - scrollbar.ScrollValue - 2;
                    break;
            }
            return new Point(x, y);
        }

        public Point CanvasMouseBlockClamped() {
            int x = (int)(drawingRegion.MouseX / (float)ColumnWidth);
            int mY = Math.Clamp(drawingRegion.MouseY, 0, 198);
            int y = 0;
            switch (currentEnvelope.Type) {
                case Envelope.EnvelopeType.Volume:
                case Envelope.EnvelopeType.WaveBlend:
                case Envelope.EnvelopeType.WaveStretch:
                case Envelope.EnvelopeType.WaveFM:
                case Envelope.EnvelopeType.WaveSync:
                    y = 99 - mY / 2;
                    break;
                case Envelope.EnvelopeType.Arpeggio:
                    if (mY > drawingRegion.height - 3) {
                        mY = drawingRegion.height - 3;
                    }

                    y = arpRange - mY / arpHeight - scrollbar.ScrollValue;
                    break;
                case Envelope.EnvelopeType.Pitch:
                    y = 99 - mY;
                    break;
                case Envelope.EnvelopeType.Wave:
                    if (mY > drawingRegion.height - 3) {
                        mY = drawingRegion.height - 3;
                    }

                    y = waveRange - mY / waveHeight - scrollbar.ScrollValue - 2;
                    break;
            }
            return currentEnvelope.Length - 1 > 0 ? new Point(Math.Clamp(x, 0, currentEnvelope.Length - 1), y) : new Point(0, y);
        }
        public void SetEnvelope(Envelope envelope, EnvelopePlayer playback) {
            if (envelope == null) {
                currentEnvelope = null;
                playbackEnvelopePlayer = null;
                return;
            }
            if (envelope != currentEnvelope) {
                ResetScrollbar();
                currentEnvelope = envelope;
                if (envelope.Type == Envelope.EnvelopeType.Arpeggio) {
                    scrollbar.SetSize(arpRange * 2 - 1, 200 / arpHeight);
                    scrollbar.ScrollValue = scrollbar.MaxScrollValue / 2 + 1;
                    scrollbar.UpdateScrollValue();
                }
                else if (envelope.Type == Envelope.EnvelopeType.Wave) {
                    scrollbar.SetSize(waveRange - 1, 200 / waveHeight);
                    scrollbar.ScrollValue = scrollbar.MaxScrollValue;
                    scrollbar.UpdateScrollValue();

                }
                else {
                    scrollbar.SetSize(1, 2);
                }
                playbackEnvelopePlayer = playback;
            }
        }

        public void Draw() {
            if (currentEnvelope != null) {
                // draw envelope editor background
                DrawRect(0, 0, width, height, UIColors.black);
                DrawRect(1, 21, MARGIN_WIDTH, 200, new Color(31, 36, 63));
                // draw loop ribbon
                DrawRect(1, 1, MARGIN_WIDTH, 9, new Color(172, 202, 162));
                DrawRect(MARGIN_WIDTH + 1, 1, width - MARGIN_WIDTH - 2, 9, new Color(14, 72, 55));
                WriteRightAlign("Loop", MARGIN_WIDTH - 3, 2, UIColors.black);
                // draw release ribbon
                DrawRect(1, 11, MARGIN_WIDTH, 9, new Color(234, 192, 165));
                DrawRect(MARGIN_WIDTH + 1, 11, width - MARGIN_WIDTH - 2, 9, new Color(125, 56, 51));
                WriteRightAlign("Release", MARGIN_WIDTH - 3, 12, UIColors.black);

                if (currentEnvelope.Length > 0) {
                    Color playbackColor = new Color(209, 244, 205);
                    bool mouseIsInDrawRegion = PointIsInCanvas(new Point(MouseX, MouseY));
                    switch (currentEnvelope.Type) {
                        case Envelope.EnvelopeType.Volume:
                        case Envelope.EnvelopeType.WaveBlend:
                        case Envelope.EnvelopeType.WaveStretch:
                        case Envelope.EnvelopeType.WaveFM:
                        case Envelope.EnvelopeType.WaveSync:
                            for (int i = 0; i < currentEnvelope.Length; ++i) {
                                if (i % 2 == 0) {
                                    DrawSprite(GetXPositionOfColumn(i), drawingRegion.y, ColumnWidth, 199, new Rectangle(392, 80, 1, 199));
                                }
                                else {
                                    DrawSprite(GetXPositionOfColumn(i), drawingRegion.y, ColumnWidth, 199, new Rectangle(393, 80, 1, 199));
                                }

                                if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y > currentEnvelope.values[i] && mouseIsInDrawRegion) {
                                    DrawMouseBlock(new Color(64, 73, 115));
                                }

                                if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y <= currentEnvelope.values[i] && mouseIsInDrawRegion) {
                                    DrawBlock(i, currentEnvelope.values[i], UIColors.selectionLight, true);
                                }
                                else if (PlaybackStep == i && IsPlaying) {
                                    DrawBlock(i, currentEnvelope.values[i], playbackColor, true);
                                }
                                else {
                                    DrawBlock(i, currentEnvelope.values[i], Color.White, true);
                                }
                            }
                            break;
                        case Envelope.EnvelopeType.Arpeggio:
                            for (int i = 0; i < currentEnvelope.Length; ++i) {
                                if (i % 2 == 0) {
                                    DrawSprite(GetXPositionOfColumn(i), drawingRegion.y, ColumnWidth, 199, new Rectangle(394, 80, 1, 199));
                                    for (int j = arpRange; j > -arpRange; j--) {
                                        if (j % 12 == 0) {
                                            DrawBlock(i, j, new Color(31, 36, 63), false);
                                        }
                                    }
                                }
                                else {
                                    DrawSprite(GetXPositionOfColumn(i), drawingRegion.y, ColumnWidth, 199, new Rectangle(395, 80, 1, 199));
                                    for (int j = arpRange; j > -arpRange; j--) {
                                        if (j % 12 == 0) {
                                            DrawBlock(i, j, new Color(42, 51, 83), false);
                                        }
                                    }
                                }
                                if (CanvasMouseBlock().X == i && mouseIsInDrawRegion) {
                                    DrawMouseBlock(new Color(64, 73, 115));
                                }

                                if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y == currentEnvelope.values[i]) {
                                    DrawBlock(i, currentEnvelope.values[i], UIColors.selectionLight, true);
                                }
                                else if (PlaybackStep == i && IsPlaying) {
                                    DrawBlock(i, currentEnvelope.values[i], playbackColor, true);
                                }
                                else {
                                    DrawBlock(i, currentEnvelope.values[i], Color.White, true);
                                }
                            }
                            break;
                        case Envelope.EnvelopeType.Pitch:
                            for (int i = 0; i < currentEnvelope.Length; ++i) {
                                if (i % 2 == 0) {
                                    DrawSprite(GetXPositionOfColumn(i), drawingRegion.y, ColumnWidth, 199, new Rectangle(396, 80, 1, 199));

                                }
                                else {
                                    DrawSprite(GetXPositionOfColumn(i), 21, ColumnWidth, 199, new Rectangle(397, 80, 1, 199));
                                }

                                Point mouse = CanvasMouseBlock();
                                if (mouse.X == i && PointIsInCanvas(new Point(MouseX, MouseY))) {
                                    DrawMouseBlock(new Color(64, 73, 115));
                                }

                                if (currentEnvelope.values[i] >= 0 && CanvasMouseBlock().X == i && CanvasMouseBlock().Y <= currentEnvelope.values[i] && mouseIsInDrawRegion && CanvasMouseBlock().Y >= 0) {
                                    DrawBlock(i, currentEnvelope.values[i], UIColors.selectionLight, true);
                                }
                                else if (currentEnvelope.values[i] < 0 && CanvasMouseBlock().X == i && CanvasMouseBlock().Y >= currentEnvelope.values[i] && mouseIsInDrawRegion && CanvasMouseBlock().Y <= 0) {
                                    DrawBlock(i, currentEnvelope.values[i], UIColors.selectionLight, true);
                                }
                                else if (PlaybackStep == i && IsPlaying) {
                                    DrawBlock(i, currentEnvelope.values[i], playbackColor, true);
                                }
                                else {
                                    DrawBlock(i, currentEnvelope.values[i], Color.White, true);
                                }
                            }
                            break;
                        case Envelope.EnvelopeType.Wave:
                            for (int i = 0; i < currentEnvelope.Length; ++i) {
                                if (i % 2 == 0) {
                                    DrawSprite(GetXPositionOfColumn(i), drawingRegion.y, ColumnWidth, 199, new Rectangle(394, 80, 1, 199));
                                    for (int j = waveRange; j > -waveRange; j--) {
                                        if (j % 10 == 0) {
                                            DrawBlock(i, j, new Color(31, 36, 63), false);
                                        }
                                    }
                                }
                                else {
                                    DrawSprite(GetXPositionOfColumn(i), drawingRegion.y, ColumnWidth, 199, new Rectangle(395, 80, 1, 199));
                                    for (int j = waveRange; j > -waveRange; j--) {
                                        if (j % 10 == 0) {
                                            DrawBlock(i, j, new Color(42, 51, 83), false);
                                        }
                                    }
                                }
                                if (CanvasMouseBlock().X == i && mouseIsInDrawRegion) {
                                    DrawMouseBlock(new Color(64, 73, 115));
                                }

                                if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y == currentEnvelope.values[i]) {
                                    DrawBlock(i, currentEnvelope.values[i], UIColors.selectionLight, true);
                                }
                                else if (PlaybackStep == i && IsPlaying) {
                                    DrawBlock(i, currentEnvelope.values[i], playbackColor, true);
                                }
                                else {
                                    DrawBlock(i, currentEnvelope.values[i], Color.White, true);
                                }
                            }
                            break;
                    }

                    DrawShiftLine();
                    string s = (int)(currentEnvelope.Length * (1000f / App.CurrentModule.TickRate)) + " ms ";
                    if (true) {
                        s += "(" + CanvasMouseBlockClamped().X + ", " + CanvasMouseBlockClamped().Y + ")";
                    }
                    Write(s, 90, 226, UIColors.label);
                    #region draw loop/release
                    // draw release
                    if (MouseIsInReleaseRibbon && MouseEnvelopeX >= 1 && MouseEnvelopeX < currentEnvelope.Length && currentEnvelope.IsActive) {
                        DrawFlagSprite(GetXPositionOfColumn(MouseEnvelopeX) - 1, 11, new Rectangle(400, 107, 40, 9));
                    }
                    if (currentEnvelope.HasRelease) {
                        if (MouseIsInReleaseRibbon && MouseEnvelopeX - 1 == currentEnvelope.ReleaseIndex) {
                            DrawFlagSprite(GetXPositionOfColumn(MouseEnvelopeX) - 1, 11, new Rectangle(400, 125, 40, 9));
                        }
                        else {
                            DrawFlagSprite(GetXPositionOfColumn(currentEnvelope.ReleaseIndex + 1) - 1, 11, new Rectangle(400, 116, 40, 9));
                        }
                        DrawRect(GetXPositionOfColumn(currentEnvelope.ReleaseIndex + 1), 20, 1, height - 21, new Color(255, 137, 51));
                    }
                    // draw loop
                    if (MouseIsInLoopRibbon && MouseEnvelopeX >= 0 && MouseEnvelopeX < currentEnvelope.Length && currentEnvelope.IsActive) {
                        DrawFlagSprite(GetXPositionOfColumn(MouseEnvelopeX) - 2, 1, new Rectangle(400, 80, 40, 9));
                    }
                    if (currentEnvelope.HasLoop) {
                        if (MouseIsInLoopRibbon && MouseEnvelopeX == currentEnvelope.LoopIndex) {
                            DrawFlagSprite(GetXPositionOfColumn(currentEnvelope.LoopIndex) - 2, 1, new Rectangle(400, 98, 40, 9));
                        }
                        else {
                            DrawFlagSprite(GetXPositionOfColumn(currentEnvelope.LoopIndex) - 2, 1, new Rectangle(400, 89, 40, 9));
                        }
                        DrawRect(GetXPositionOfColumn(currentEnvelope.LoopIndex) - 1, 10, 1, height - 11, new Color(99, 171, 63));
                    }
                    #endregion
                    switch (currentEnvelope.Type) {
                        case Envelope.EnvelopeType.Volume:
                        case Envelope.EnvelopeType.WaveBlend:
                        case Envelope.EnvelopeType.WaveStretch:
                        case Envelope.EnvelopeType.WaveFM:
                        case Envelope.EnvelopeType.WaveSync:
                            WriteRightAlign("99", drawingRegion.x - 4, drawingRegion.y - 1, Color.White);
                            WriteRightAlign("00", drawingRegion.x - 4, drawingRegion.y + drawingRegion.height - 8, Color.White);
                            break;
                        case Envelope.EnvelopeType.Arpeggio:
                            string valUpper = "" + (arpRange - scrollbar.ScrollValue);
                            string valLower = "" + (arpRange - scrollbar.ScrollValue - 200 / arpHeight + 1);
                            WriteRightAlign(valUpper, drawingRegion.x - 4, drawingRegion.y - 1, Color.White);
                            WriteRightAlign(valLower, drawingRegion.x - 4, drawingRegion.y + drawingRegion.height - 8, Color.White);
                            break;
                        case Envelope.EnvelopeType.Pitch:
                            WriteRightAlign("99", drawingRegion.x - 4, drawingRegion.y - 1, Color.White);
                            WriteRightAlign("-100", drawingRegion.x - 4, drawingRegion.y + drawingRegion.height - 8, Color.White);
                            break;
                        case Envelope.EnvelopeType.Wave:
                            valUpper = "" + (waveRange - scrollbar.ScrollValue - 2);
                            valLower = "" + (waveRange - scrollbar.ScrollValue - 200 / waveHeight - 1);
                            WriteRightAlign(valUpper, drawingRegion.x - 4, drawingRegion.y - 1, Color.White);
                            WriteRightAlign(valLower, drawingRegion.x - 4, drawingRegion.y + drawingRegion.height - 8, Color.White);
                            break;
                    }
                }
                envText.Draw();
                envLength.Draw();
                if (currentEnvelope.Type is Envelope.EnvelopeType.Arpeggio or
                    Envelope.EnvelopeType.Wave) {
                    scrollbar.Draw();
                }
                if (!currentEnvelope.IsActive) {
                    DrawRect(0, 0, width, envText.height + envText.y, new Color(255, 255, 255, 150));
                }
            }
            else {
                DrawRect(0, 0, width, envText.height + envText.y, UIColors.panel);
                WriteCenter("No envelope selected", width / 2, height / 2, UIColors.label);
                WriteCenter("Click \"Add Envelope\" to start editing", width / 2, height / 2 + 16, UIColors.label);
            }
        }

        private void DrawFlagSprite(int x, int y, Rectangle spriteRect) {
            if (x + spriteRect.Width > width - 1) {
                spriteRect.Width -= x + spriteRect.Width - (width - 1);
            }
            DrawSprite(x, y, spriteRect);
        }

        private void DrawMouseBlock(Color c) {
            Point p = CanvasMouseBlockClamped();
            DrawBlock(p.X, p.Y, c, false);
        }

        private void DrawShiftLine() {
            if (PointIsInCanvas(LastClickPos) && Input.GetClick(KeyModifier.Shift)) {
                if (Input.GetClick(KeyModifier.Shift)) {
                    int canvasPosX = CanvasMouseBlockClamped().X;
                    int canvasPosY = CanvasMouseBlockClamped().Y;
                    int diff = Math.Abs(holdPosX - canvasPosX);
                    if (diff > 0) {
                        if (holdPosX < canvasPosX) {
                            for (int i = holdPosX; i <= canvasPosX; ++i) {
                                int y = (int)Math.Round(Lerp(holdPosY, canvasPosY, (float)(i - holdPosX) / diff));
                                DrawBlock(i, y, new Color(100, 90, 135, 90), false);
                            }
                        }
                        else {
                            for (int i = canvasPosX; i <= holdPosX; ++i) {
                                int y = (int)Math.Round(Lerp(canvasPosY, holdPosY, (float)(i - canvasPosX) / diff));
                                DrawBlock(i, y, new Color(100, 90, 135, 90), false);
                            }
                        }
                    }
                }
            }
        }

        public int MouseEnvelopeX {

            get { return (int)Math.Floor(drawingRegion.MouseX / (float)ColumnWidth); }
        }

        private bool MouseIsInLoopRibbon { get { return MouseY >= 0 && MouseY <= 8; } }

        private bool MouseIsInReleaseRibbon { get { return MouseY >= 10 && MouseY <= 18; } }

        private bool PointIsInCanvas(Point p) {
            if (currentEnvelope.IsActive) {
                if (p.Y >= drawingRegion.y && p.Y < drawingRegion.y + drawingRegion.height) {
                    if (p.X >= drawingRegion.x && p.X < currentEnvelope.Length * ColumnWidth + drawingRegion.x) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void DrawBlock(int i, int val, Color c, bool shadow) {
            switch (currentEnvelope.Type) {
                case Envelope.EnvelopeType.Volume:
                case Envelope.EnvelopeType.WaveBlend:
                case Envelope.EnvelopeType.WaveStretch:
                case Envelope.EnvelopeType.WaveFM:
                case Envelope.EnvelopeType.WaveSync:
                    DrawRect(GetXPositionOfColumn(i), GetYPositionOfValue(val), ColumnWidth, val * 2 + 1, c);
                    if (shadow && ColumnWidth > 1) {
                        DrawRect(GetXPositionOfColumn(i) + ColumnWidth - 1, GetYPositionOfValue(val), 1, val * 2 + 1, Color.LightGray);
                    }

                    break;
                case Envelope.EnvelopeType.Pitch:
                    int height = val;
                    int y = 0;
                    if (val > 0) {
                        height++;
                    }

                    if (val <= 0) {
                        y++;
                        height--;
                    }
                    DrawRect(GetXPositionOfColumn(i), GetYPositionOfValue(val) + y, ColumnWidth, height, c);
                    if (shadow && ColumnWidth > 1) {
                        DrawRect(GetXPositionOfColumn(i) + ColumnWidth - 1, GetYPositionOfValue(val) + y, 1, height, Color.LightGray);
                    }

                    break;
                case Envelope.EnvelopeType.Arpeggio:
                case Envelope.EnvelopeType.Wave:
                    if (GetYPositionOfValue(val) > 20 && GetYPositionOfValue(val) < 219) {
                        DrawRect(GetXPositionOfColumn(i), GetYPositionOfValue(val), ColumnWidth, arpHeight, c);
                        if (shadow && ColumnWidth > 1) {
                            DrawRect(GetXPositionOfColumn(i) + ColumnWidth - 1, GetYPositionOfValue(val), 1, arpHeight, Color.LightGray);
                        }
                    }
                    break;
            }
        }
    }
}
