using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class SetFramePatternDialog : Dialog {
        Button cancel, ok;
        NumberBox patternIndex;
        WTFrame parentFrame;
        public SetFramePatternDialog() : base("Set Pattern...", 146, 58) {
            PositionInCenterOfScreen();
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            patternIndex = new NumberBox("Pattern Index", 8, 19, 132, 36, this);
            patternIndex.SetValueLimits(0, 99);
            patternIndex.Value = 5;
        }

        public void Open(WTFrame frame) {
            parentFrame = frame;
            patternIndex.Value = parentFrame.PatternIndex;
            Open();
        }

        public override void Update() {
            if (WindowIsOpen) {
                DoDragging();
                if (cancel.Clicked || ExitButton.Clicked)
                    Close();
                if (ok.Clicked) {
                    parentFrame.PatternIndex = patternIndex.Value;
                    Close();
                }
                patternIndex.Update();
            }
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                patternIndex.Draw();
            }
        }
    }
}
