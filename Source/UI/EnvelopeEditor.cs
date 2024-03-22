using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using WaveTracker.Audio;
using WaveTracker.Rendering;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class EnvelopeEditor : Clickable {
        Envelope envelope;
        Scrollbar scrollbar;
        int envelopeType;
        int playbackStep;
        bool isPlaying;
        Textbox envText;
        int arpRange = 120;
        int arpHeight = 6;
        int waveRange = 101;
        int waveHeight = 6;
        NumberBox envLength;
        public int lastEnvType;
        int holdPosX, holdPosY;
        int ColumnWidth => (envelope.values.Count == 0 ? 0 : Math.Clamp(445 / envelope.values.Count, 1, 48));
        public EnvelopeEditor(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            envText = new("", -1, 239, 535, 535, this);
            envText.maxLength = 256;
            scrollbar = new(44, 20, 489, 200, this);
            scrollbar.CoarseStepAmount = 2;
            envLength = new("Length", -1, 223, 74, 38, this);

            envLength.SetValueLimits(0, 220);

            SetParent(parent);
            ResetScrollbar();

        }

        public void ResetScrollbar() {
            scrollbar.ScrollValue = arpRange - 16;
            scrollbar.Update();
        }

        int xPositionOfColumn(int index) { return 46 + index * ColumnWidth; }
        int yPositionOfValue(int value) {
            if (envelopeType == 0 || envelopeType == 4) // vol + wave mod
            {
                return 21 + (99 - value) * 2;
            }
            else if (envelopeType == 1) // arp
              {
                return 21 + (arpRange - value - scrollbar.ScrollValue) * arpHeight;
            }
            else if (envelopeType == 3) // wave
              {
                return 21 + (waveRange - value - scrollbar.ScrollValue - 2) * waveHeight;
            }
            else  // pitch
              {
                return 20 + (99 - value);
            }
        }

        public void Update() {


            envLength.Value = envelope.values.Count;
            envLength.Update();
            if (envLength.ValueWasChangedInternally) {
                App.CurrentModule.SetDirty();
            }
            envLength.enabled = envelope.isActive;
            envText.enabled = envelope.isActive;
            if (envLength.ValueWasChanged) {
                while (envLength.Value < envelope.values.Count)
                    envelope.values.RemoveAt(envelope.values.Count - 1);
                while (envLength.Value > envelope.values.Count)
                    envelope.values.Add(0);
            }
            if (envelope.isActive)
                scrollbar.Update();
            if (envelopeType == 0)
                scrollbar.SetSize(1, 2);
            if (envelopeType == 1) {
                scrollbar.SetSize(arpRange * 2 - 1, 200 / arpHeight);
            }
            if (envelopeType == 2)
                scrollbar.SetSize(1, 2);
            if (envelopeType == 3) {
                scrollbar.SetSize(waveRange - 1, 200 / waveHeight);
            }
            envText.Text = envelope.ToString();
            envText.Update();
            if (envText.ValueWasChangedInternally) {
                envelope.LoadFromString(envText.Text, envelopeType);
                App.CurrentModule.SetDirty();
            }
            envText.Text = envelope.ToString();
            int canvasPosX = CanvasMouseBlockClamped().X;
            int canvasPosY = CanvasMouseBlockClamped().Y;
            if (PointIsInCanvas(LastClickPos)) {

                if (Input.GetClickDown(KeyModifier._Any)) {
                    holdPosX = CanvasMouseBlockClamped().X;
                    holdPosY = CanvasMouseBlockClamped().Y;
                }
                if (Input.GetClick(KeyModifier.None)) {
                    envelope.values[Math.Clamp(canvasPosX, 0, envelope.values.Count - 1)] = (short)canvasPosY;
                    App.CurrentModule.SetDirty();
                }

                if (Input.GetClickUp(KeyModifier.Shift)) {
                    int diff = Math.Abs(holdPosX - canvasPosX);
                    if (diff > 0) {
                        if (holdPosX < canvasPosX) {
                            for (int i = holdPosX; i <= canvasPosX; ++i) {
                                envelope.values[i] = (short)Math.Round(Lerp(holdPosY, canvasPosY, (float)(i - holdPosX) / diff));
                            }
                        }
                        else {
                            for (int i = canvasPosX; i <= holdPosX; ++i) {
                                envelope.values[i] = (short)Math.Round(Lerp(canvasPosY, holdPosY, (float)(i - canvasPosX) / diff));
                            }
                        }
                    }
                    else {
                        envelope.values[Math.Clamp(canvasPosX, 0, envelope.values.Count - 1)] = (short)canvasPosY;
                    }
                    App.CurrentModule.SetDirty();
                }

            }

            // input loop/release
            #region loop/release
            if (Input.GetClickDown(KeyModifier.None) && envelope.isActive) {
                if (MouseEnvelopeY == 1 && MouseEnvelopeX >= 1 && MouseEnvelopeX < envelope.values.Count) {
                    if (envelope.releaseIndex != MouseEnvelopeX - 1)
                        envelope.releaseIndex = MouseEnvelopeX - 1;
                    else
                        envelope.releaseIndex = Envelope.emptyEnvValue;
                    App.CurrentModule.SetDirty();
                }
                if (MouseEnvelopeY == 0 && MouseEnvelopeX >= 0 && MouseEnvelopeX < envelope.values.Count) {
                    if (envelope.loopIndex != MouseEnvelopeX)
                        envelope.loopIndex = MouseEnvelopeX;
                    else
                        envelope.loopIndex = Envelope.emptyEnvValue;
                    App.CurrentModule.SetDirty();
                }
            }
            #endregion
        }

        float Lerp(float firstFloat, float secondFloat, float by) {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public Point CanvasMouseBlock() {
            int x = (int)Math.Floor((MouseX - 46) / (float)ColumnWidth);
            int y;
            if (envelopeType == 0 || envelopeType == 4) // vol + wave mod
            {
                y = 99 - (MouseY - 21) / 2;
            }
            else if (envelopeType == 1) // arp
              {
                y = arpRange - ((MouseY - 21) / arpHeight) - scrollbar.ScrollValue;
            }
            else if (envelopeType == 3) // wave
              {
                y = waveRange - ((MouseY - 21) / waveHeight) - scrollbar.ScrollValue - 2;
            }
            else // pitch
              {
                y = 98 - (MouseY - 21);
            }
            return new Point(x, y);
        }

        public Point CanvasMouseBlockClamped() {
            int mY = Math.Clamp(MouseY, 20, 219);

            int y;

            if (envelopeType == 0 || envelopeType == 4) // vol + wave mod
            {
                y = 99 - (mY - 21) / 2;
            }
            else if (envelopeType == 1) // arp
              {
                if (mY > 218)
                    mY = 218;
                y = arpRange - ((mY - 21) / arpHeight) - scrollbar.ScrollValue;
            }
            else if (envelopeType == 3) // wave
              {
                if (mY > 218)
                    mY = 218;
                y = waveRange - ((mY - 21) / waveHeight) - scrollbar.ScrollValue - 2;
            }
            else // pitch
              {
                y = 98 - (mY - 21);
            }
            if (envelope.values.Count - 1 > 0)
                return new Point(Math.Clamp(CanvasMouseBlock().X, 0, envelope.values.Count - 1), y);
            return new Point(0, y);
            //return new Point(x, y);
        }

        public void SetEnvelope(Envelope envelope, int envelopeType) {
            if (lastEnvType != this.envelopeType) {
                ResetScrollbar();
                lastEnvType = this.envelopeType;
            }
            this.envelope = envelope;
            this.envelopeType = envelopeType;
        }
        public void Draw() {

            if (envelope != null) {
                if (envelope.values.Count > 0) {
                    if (envelopeType == 0 || envelopeType == 4) // volume + wave mod
                    {
                        for (int i = 0; i < envelope.values.Count; ++i) {
                            if (i % 2 == 0)
                                DrawSprite(xPositionOfColumn(i), 21, ColumnWidth, 199, new Rectangle(392, 80, 1, 199));
                            else
                                DrawSprite(xPositionOfColumn(i), 21, ColumnWidth, 199, new Rectangle(393, 80, 1, 199));

                            if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y > envelope.values[i] && PointIsInCanvas(new Point(MouseX, MouseY)))
                                DrawMouseBlock(new Color(64, 73, 115));


                            if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y <= envelope.values[i] && PointIsInCanvas(new Point(MouseX, MouseY))) {
                                DrawBlock(i, envelope.values[i], new Color(193, 222, 235), true);
                            }
                            else if (playbackStep == i && isPlaying)
                                DrawBlock(i, envelope.values[i], new Color(209, 244, 205), true);

                            else
                                DrawBlock(i, envelope.values[i], Color.White, true);

                        }


                    }
                    else if (envelopeType == 1) // arp
                      {
                        for (int i = 0; i < envelope.values.Count; ++i) {
                            if (i % 2 == 0) {
                                DrawSprite(xPositionOfColumn(i), 21, ColumnWidth, 199, new Rectangle(394, 80, 1, 199));
                                for (int j = arpRange; j > -arpRange; j--) {
                                    if (j % 12 == 0)
                                        DrawBlock(i, j, new Color(31, 36, 63), false);
                                }
                            }
                            else {
                                DrawSprite(xPositionOfColumn(i), 21, ColumnWidth, 199, new Rectangle(395, 80, 1, 199));
                                for (int j = arpRange; j > -arpRange; j--) {
                                    if (j % 12 == 0)
                                        DrawBlock(i, j, new Color(42, 51, 83), false);
                                }
                            }
                            if (CanvasMouseBlock().X == i && PointIsInCanvas(new Point(MouseX, MouseY)))
                                DrawMouseBlock(new Color(64, 73, 115));

                            if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y == envelope.values[i]) {
                                DrawBlock(i, envelope.values[i], new Color(193, 222, 235), true);
                            }
                            else if (playbackStep == i && isPlaying)
                                DrawBlock(i, envelope.values[i], new Color(209, 244, 205), true);
                            else
                                DrawBlock(i, envelope.values[i], Color.White, true);
                        }
                    }
                    else if (envelopeType == 3) // wave
                      {
                        for (int i = 0; i < envelope.values.Count; ++i) {
                            if (i % 2 == 0) {
                                DrawSprite(xPositionOfColumn(i), 21, ColumnWidth, 199, new Rectangle(394, 80, 1, 199));
                                for (int j = waveRange; j > -waveRange; j--) {
                                    if (j % 10 == 0)
                                        DrawBlock(i, j, new Color(31, 36, 63), false);
                                }
                            }
                            else {
                                DrawSprite(xPositionOfColumn(i), 21, ColumnWidth, 199, new Rectangle(395, 80, 1, 199));
                                for (int j = waveRange; j > -waveRange; j--) {
                                    if (j % 10 == 0)
                                        DrawBlock(i, j, new Color(42, 51, 83), false);
                                }
                            }
                            if (CanvasMouseBlock().X == i && PointIsInCanvas(new Point(MouseX, MouseY)))
                                DrawMouseBlock(new Color(64, 73, 115));

                            if (CanvasMouseBlock().X == i && CanvasMouseBlock().Y == envelope.values[i]) {
                                DrawBlock(i, envelope.values[i], new Color(193, 222, 235), true);
                            }
                            else if (playbackStep == i && isPlaying)
                                DrawBlock(i, envelope.values[i], new Color(209, 244, 205), true);
                            else
                                DrawBlock(i, envelope.values[i], Color.White, true);
                        }
                    }
                    else if (envelopeType == 2) // pitch
                      {
                        for (int i = 0; i < envelope.values.Count; ++i) {
                            if (i % 2 == 0) {
                                DrawSprite(xPositionOfColumn(i), 20, ColumnWidth, 199, new Rectangle(396, 80, 1, 199));

                            }
                            else
                                DrawSprite(xPositionOfColumn(i), 20, ColumnWidth, 199, new Rectangle(397, 80, 1, 199));

                            Point mouse = CanvasMouseBlock();
                            if (mouse.X == i && PointIsInCanvas(new Point(MouseX, MouseY)))
                                DrawMouseBlock(new Color(64, 73, 115));



                            if (envelope.values[i] >= 0 && CanvasMouseBlock().X == i && CanvasMouseBlock().Y <= envelope.values[i] && PointIsInCanvas(new Point(MouseX, MouseY)) && CanvasMouseBlock().Y >= 0) {
                                DrawBlock(i, envelope.values[i], new Color(193, 222, 235), true);
                            }
                            else if (envelope.values[i] < 0 && CanvasMouseBlock().X == i && CanvasMouseBlock().Y > envelope.values[i] && PointIsInCanvas(new Point(MouseX, MouseY)) && CanvasMouseBlock().Y <= 0) {
                                DrawBlock(i, envelope.values[i], new Color(193, 222, 235), true);
                            }
                            else if (playbackStep == i && isPlaying)
                                DrawBlock(i, envelope.values[i], new Color(209, 244, 205), true);
                            else
                                DrawBlock(i, envelope.values[i], Color.White, true);

                        }
                    }

                    DrawShiftLine();
                    string s = (int)(envelope.values.Count * (1000f / App.CurrentModule.TickRate)) + " ms ";
                    if (true) {
                        s += "(" + CanvasMouseBlockClamped().X + ", " + CanvasMouseBlockClamped().Y + ")";

                    }
                    Write(s, 90, 226, UIColors.label);
                    #region draw loop/release
                    // draw release
                    if (MouseEnvelopeY == 1 && MouseEnvelopeX >= 1 && MouseEnvelopeX < envelope.values.Count && envelope.isActive) {
                        DrawSprite(xPositionOfColumn(MouseEnvelopeX) - 1, 10, new Rectangle(400, 107, 40, 9));
                    }
                    if (envelope.HasRelease) {
                        if (MouseEnvelopeY == 1 && MouseEnvelopeX - 1 == envelope.releaseIndex) {
                            DrawSprite(xPositionOfColumn(MouseEnvelopeX) - 1, 10, new Rectangle(400, 125, 40, 9));
                        }
                        else {
                            DrawSprite(xPositionOfColumn(envelope.releaseIndex + 1) - 1, 10, new Rectangle(400, 116, 40, 9));
                        }
                        DrawRect(xPositionOfColumn(envelope.releaseIndex + 1), 19, 1, 201, new Color(255, 137, 51));
                    }
                    // draw loop
                    if (MouseEnvelopeY == 0 && MouseEnvelopeX >= 0 && MouseEnvelopeX < envelope.values.Count && envelope.isActive) {
                        DrawSprite(xPositionOfColumn(MouseEnvelopeX) - 2, 0, new Rectangle(400, 80, 40, 9));
                    }
                    if (envelope.HasLoop) {
                        if (MouseEnvelopeY == 0 && MouseEnvelopeX == envelope.loopIndex) {
                            DrawSprite(xPositionOfColumn(envelope.loopIndex) - 2, 0, new Rectangle(400, 98, 40, 9));
                        }
                        else {
                            DrawSprite(xPositionOfColumn(envelope.loopIndex) - 2, 0, new Rectangle(400, 89, 40, 9));
                        }
                        DrawRect(xPositionOfColumn(envelope.loopIndex) - 1, 9, 1, 211, new Color(99, 171, 63));
                    }
                    #endregion
                    switch (envelopeType) {
                        case 0:
                        case 4:
                            Write(" 99", 29, 20, Color.White);
                            Write(" 00", 29, 213, Color.White);
                            break;
                        case 1:
                            string valUpper = "" + (arpRange - scrollbar.ScrollValue);
                            string valLower = "" + (arpRange - scrollbar.ScrollValue - (200 / arpHeight) + 1);
                            while (valUpper.Length < 3)
                                valUpper = " " + valUpper;
                            while (valLower.Length < 3)
                                valLower = " " + valLower;
                            Write(valUpper, 29, 20, Color.White);
                            Write(valLower, 29, 213, Color.White);
                            break;
                        case 2:
                            Write(" 99", 29, 20, Color.White);
                            Write("-100", 29, 213, Color.White);
                            break;
                        case 3:
                            valUpper = "" + (waveRange - scrollbar.ScrollValue - 2);
                            valLower = "" + (waveRange - scrollbar.ScrollValue - (200 / waveHeight) - 1);
                            while (valUpper.Length < 3)
                                valUpper = " " + valUpper;
                            while (valLower.Length < 3)
                                valLower = " " + valLower;
                            Write(valUpper, 29, 20, Color.White);
                            Write(valLower, 29, 213, Color.White);
                            break;
                    }
                }
                envText.Draw();
                envLength.Draw();
                if (envelopeType == 1 || envelopeType == 3)
                    scrollbar.Draw();
                if (!envelope.isActive) {
                    DrawRect(-1, -1, 535, 253, new Color(255, 255, 255, 100));
                }
            }
        }
        void DrawMouseBlock(Color c) {
            DrawBlock(CanvasMouseBlockClamped().X, CanvasMouseBlockClamped().Y, c, false);
        }

        void DrawShiftLine() {
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

            get { return (int)Math.Floor((MouseX - 46) / (float)ColumnWidth); }
        }
        public int MouseEnvelopeY {
            get {
                if (MouseY >= 0 && MouseY <= 8)
                    return 0;
                if (MouseY >= 10 && MouseY <= 18)
                    return 1;
                return -1;

            }
        }


        bool PointIsInCanvas(Point p) {
            if (envelope.isActive)
                if (p.Y > 20 && p.Y < 219)
                    if (p.X > 40 && p.X < envelope.values.Count * ColumnWidth + 46)
                        return true;
            return false;
        }
        void DrawBlock(int i, int val, Color c, bool shadow) {
            if (envelopeType == 0 || envelopeType == 4) // volume + wave mod
            {
                DrawRect(xPositionOfColumn(i), yPositionOfValue(val), ColumnWidth, val * 2 + 1, c);
                if (shadow)
                    DrawRect(xPositionOfColumn(i) + ColumnWidth - 1, yPositionOfValue(val), 1, val * 2 + 1, Color.LightGray);
            }

            if (envelopeType == 1 || envelopeType == 3) // arp + wave select
            {
                if (yPositionOfValue(val) > 20 && yPositionOfValue(val) < 219) {
                    DrawRect(xPositionOfColumn(i), yPositionOfValue(val), ColumnWidth, arpHeight, c);
                    if (shadow)
                        DrawRect(xPositionOfColumn(i) + ColumnWidth - 1, yPositionOfValue(val), 1, arpHeight, Color.LightGray);
                }
            }

            if (envelopeType == 2) // pitch
            {
                int height = val;
                int y = 0;
                if (val > 0)
                    height++;
                if (val <= 0) {
                    y++;
                    height--;
                }
                DrawRect(xPositionOfColumn(i), yPositionOfValue(val) + y, ColumnWidth, height, c);
                if (shadow)
                    DrawRect(xPositionOfColumn(i) + ColumnWidth - 1, yPositionOfValue(val) + y, 1, height, Color.LightGray);
            }
        }

        public void EditEnvelope(Envelope envelope, int envelopeType, EnvelopePlayer playback) {
            SetEnvelope(envelope, envelopeType);
            playbackStep = playback.step;
            isPlaying = !playback.EnvelopeEnded;
            Update();
        }
    }
}
