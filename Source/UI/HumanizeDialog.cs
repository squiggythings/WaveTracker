using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.UI {
    public class HumanizeDialog : Dialog {
        PatternEditor parentEditor;
        Button cancel, ok;
        NumberBox volumeRange;
        public HumanizeDialog() : base("Humanize Volumes", 146, 58) {
            InitializeInCenterOfScreen();
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            volumeRange = new NumberBox("Randomization Range", 8, 19, 132, 36, this);
            volumeRange.SetValueLimits(0, 99);
            volumeRange.SetTooltip("How much to randomize volumes in this selection");
            volumeRange.Value = 5;
        }

        public void Open(PatternEditor parentEditor) {
            this.parentEditor = parentEditor;
            Open(Input.focus);
        }

        public void Update() {
            if (windowIsEnabled) {
                if (cancel.Clicked || ExitButton.Clicked)
                    Close();
                if (ok.Clicked) {
                    parentEditor.RandomizeSelectedVolumes(volumeRange.Value);
                    Close();
                }
                volumeRange.Update();
            }
        }

        public new void Draw() {
            if (windowIsEnabled) {
                base.Draw();
                volumeRange.Draw();
            }
        }
    }
}
