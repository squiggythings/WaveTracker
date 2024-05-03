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
    public class WaveAddFuzzDialog : WaveModifyDialog {
        NumberBox fuzzAmt;
        CheckboxLabeled wrapAround;
        Button newSeed;
        Random rand;
        float[] values;

        public WaveAddFuzzDialog() : base("Add Fuzz...") {
            rand = new Random();
            fuzzAmt = new NumberBox("Amount", 8, 25, 94, 46, this);
            fuzzAmt.SetValueLimits(0, 100);
            fuzzAmt.DisplayMode = NumberBox.NumberDisplayMode.Percent;
            wrapAround = new CheckboxLabeled("Wrap values", 8, 40, 94, this);
            wrapAround.ShowCheckboxOnRight = true;

            newSeed = new Button("New seed", 8, 57, this);
            values = new float[64];
        }

        public override void Update() {
            if (windowIsOpen) {
                base.Update();
                fuzzAmt.Update();
                wrapAround.Update();
                if (newSeed.Clicked) {
                    DoNewSeed();
                }
                if (fuzzAmt.ValueWasChangedInternally)
                    Apply();
                if (wrapAround.Clicked)
                    Apply();
            }
        }

        public new void Open(Wave wave) {
            DoNewSeed();
            base.Open(wave);

        }

        void DoNewSeed() {
            for (int i = 0; i < 64; ++i) {
                values[i] = (float)rand.NextDouble() * 2 - 1;
            }
            Apply();
        }

        protected override byte GetSampleValue(int index) {
            int samp = originalData[index];
            int sign = Math.Sign(values[index]);
            for (int j = 0; j < Math.Abs((int)(values[index] * fuzzAmt.Value / 100f * 32)); ++j) {
                if (samp + sign > 31 || samp + sign < 0) {
                    if (wrapAround.Value)
                        sign *= -1;
                    else
                        break;
                }
                samp += sign;
            }
            return (byte)samp;
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                fuzzAmt.Draw();
                wrapAround.Draw();
                newSeed.Draw();
            }
        }
    }
}
