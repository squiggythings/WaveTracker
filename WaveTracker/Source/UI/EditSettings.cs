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
            if (App.Shortcuts["General\\Increase step"].IsPressedDown) {
                App.PatternEditor.InputStep = Math.Clamp(App.PatternEditor.InputStep + 1, 0, 256);
            }
            if (App.Shortcuts["General\\Decrease step"].IsPressedDown) {
                App.PatternEditor.InputStep = Math.Clamp(App.PatternEditor.InputStep - 1, 0, 256);
            }

            if (octave.ValueWasChangedInternally) {
                App.PatternEditor.CurrentOctave = octave.Value;
                PianoInput.ClearAllNotes();
            }
            else {
                octave.Value = App.PatternEditor.CurrentOctave;
            }
            octave.Update();

            if (step.ValueWasChangedInternally) {
                App.PatternEditor.InputStep = step.Value;
            }
            else {
                step.Value = App.PatternEditor.InputStep;
            }
            step.Update();

            if (highlightPrimary.ValueWasChangedInternally) {
                App.CurrentSong.RowHighlightPrimary = highlightPrimary.Value;
            }
            else {
                highlightPrimary.Value = App.CurrentSong.RowHighlightPrimary;
            }
            highlightPrimary.Update();

            if (highlightSecondary.ValueWasChangedInternally) {
                App.CurrentSong.RowHighlightSecondary = highlightSecondary.Value;
            }
            else {
                highlightSecondary.Value = App.CurrentSong.RowHighlightSecondary;
            }
            highlightSecondary.Update();

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
        }
    }
}
