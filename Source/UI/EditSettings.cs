using Microsoft.Xna.Framework;
using System;

namespace WaveTracker.UI {
    public class EditSettings : Panel {
        private NumberBox octave;
        private NumberBox step;
        private Toggle instrumentMask;
        private NumberBox highlightPrimary;
        private NumberBox highlightSecondary;
        public EditSettings(int x, int y) : base("Edit Settings", x, y, 194, 84) {
            octave = new NumberBox("Octave", 5, 24, 80, 40, this);
            octave.bUp.SetTooltip("", "Increase current octave - ]");
            octave.bDown.SetTooltip("", "Decrease current octave - [");
            step = new NumberBox("Step", 5, 39, 80, 40, this);
            step.bUp.SetTooltip("", "Increase step size - Ctrl+]");
            step.bDown.SetTooltip("", "Decrease step size - Ctrl+[");
            instrumentMask = new Toggle("Instrument Mask", 5, 61, this);
            instrumentMask.SetTooltip("", "Bypass auto-placement of instrument column when inputting notes");
            highlightPrimary = new NumberBox("Primary", 100, 24, 88, 40, this);
            highlightSecondary = new NumberBox("Secondary", 100, 39, 88, 40, this);
            octave.SetValueLimits(0, 9);
            step.SetValueLimits(0, 256);
            highlightPrimary.SetValueLimits(1, 256);
            highlightSecondary.SetValueLimits(1, 256);
        }

        public void Update() {
            octave.Update();
            step.Update();
            highlightPrimary.Update();
            highlightSecondary.Update();
            if (octave.ValueWasChangedInternally) {
                App.PatternEditor.CurrentOctave = octave.Value;
                PianoInput.ClearAllNotes();
            }
            else {
                octave.Value = App.PatternEditor.CurrentOctave;
            }

            if (App.Shortcuts["General\\Increase step"].IsPressedDown) {
                App.PatternEditor.InputStep = Math.Clamp(App.PatternEditor.InputStep + 1, 0, 256);
            }
            if (App.Shortcuts["General\\Decrease step"].IsPressedDown) {
                App.PatternEditor.InputStep = Math.Clamp(App.PatternEditor.InputStep - 1, 0, 256);
            }
            if (step.ValueWasChangedInternally) {
                App.PatternEditor.InputStep = step.Value;
            }
            else {
                step.Value = App.PatternEditor.InputStep;
            }

            if (highlightPrimary.ValueWasChangedInternally) {
                App.CurrentSong.RowHighlightPrimary = highlightPrimary.Value;
            }
            else {
                highlightPrimary.Value = App.CurrentSong.RowHighlightPrimary;
            }

            if (highlightSecondary.ValueWasChangedInternally) {
                App.CurrentSong.RowHighlightSecondary = highlightSecondary.Value;
            }
            else {
                highlightSecondary.Value = App.CurrentSong.RowHighlightSecondary;
            }

            instrumentMask.Update();
            App.PatternEditor.InstrumentMask = instrumentMask.Value;
        }

        public new void Draw() {
            base.Draw();
            octave.Draw();
            step.Draw();
            highlightPrimary.Draw();
            highlightSecondary.Draw();
            instrumentMask.Draw();
            DrawRect(93, 14, 1, 63, new Color(163, 167, 194));
            Write("Note Input", 5, 15, new Color(163, 167, 194));
            Write("Row Highlight", 100, 15, new Color(163, 167, 194));

            ////*********************************************************
            //int winW = 300;
            //int winH = 400;
            //DrawRect(0, 0, winW, winH, Color.Red);
            //int numChannels = App.CurrentSong.RowsPerFrame;
            //int desiredNumX = 2;
            //int desiredNumY = 4;
            //int numX = 2;
            //int numY = numChannels / 2;
            //float prevDiff = 9999999999;
            //List<int> factors = new List<int>();
            //int max = (int)Math.Sqrt(24);  // Round down

            //for (int factor = 1; factor <= max; ++factor) // Test from 1 to the square root, or the int below it, inclusive.
            //{
            //    if (numChannels % factor == 0) {
            //        int w = factor;
            //        int h = numChannels / factor;
            //        if (Math.Abs((w / (float)h) - (desiredNumX / (float)desiredNumY)) < prevDiff) {
            //            prevDiff = Math.Abs((w / (float)h) - (desiredNumX / (float)desiredNumY));
            //            numX = w;
            //            numY = h;
            //        }

            //        factors.Add(factor);
            //        if (factor != numChannels / factor) // Don't add the square root twice!  Thanks Jon
            //            factors.Add(numChannels / factor);
            //    }
            //    if (factor >= numChannels)
            //        break;
            //}

            //int subWidth = winW / numX;
            //int subHeight = winH / numY;

            //int num = 0;
            //for (int r = 0; r < numY; ++r) {
            //    for (int c = 0; c < numX; ++c) {
            //        DrawRect(c * subWidth, r * subHeight, subWidth - 1, subHeight - 1, Color.Green);
            //        num++;
            //        if (num >= numChannels)
            //            break;
            //    }
            //    if (num >= numChannels)
            //        break;
            //}
        }
    }
}
