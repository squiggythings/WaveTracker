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
        SpriteButton closeX;
        NumberBox volumeRange;
        public HumanizeDialog() {
            InitializeDialogCentered("Humanize Volumes", 146, 58);
            closeX = newCloseButton();
            cancel = newBottomButton("Cancel", this);
            ok = newBottomButton("OK", this);
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
            if (enabled) {
                if (cancel.Clicked || closeX.Clicked)
                    Close();
                if (ok.Clicked) {
                    parentEditor.RandomizeSelectedVolumes(volumeRange.Value);
                    Close();
                }
                volumeRange.Update();
            }
        }

        public void Draw() {
            if (enabled) {
                DrawDialog();
                volumeRange.Draw();
                cancel.Draw();
                ok.Draw();
                closeX.Draw();
            }
        }
    }
}
