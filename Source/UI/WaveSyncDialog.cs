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
    public class WaveSyncDialog : WaveModifyDialog {
        NumberBox harmonicNum;

        public WaveSyncDialog() : base("Set Harmonic...") {
            harmonicNum = new NumberBox("Harmonic", 8, 25, 94, 40, this);
            harmonicNum.SetValueLimits(1, 32);

        }

        public new void Open(Wave wave) {
            harmonicNum.Value = 1;
            base.Open(wave);
        }

        public override void Update() {
            if (windowIsOpen) {
                base.Update();
                harmonicNum.Update();
                if (harmonicNum.ValueWasChangedInternally)
                    Apply();
            }
        }

        protected override byte GetSampleValue(int index) {
            return originalData[(index * harmonicNum.Value) % 64];
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                harmonicNum.Draw();
            }
        }
    }
}
