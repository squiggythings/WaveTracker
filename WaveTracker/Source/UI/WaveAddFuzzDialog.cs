using System;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveAddFuzzDialog : WaveModifyDialog {
        private NumberBox fuzzAmt;
        private CheckboxLabeled wrapAround;
        private Button newSeed;
        private Random rand;
        private float[] randomValues;

        public WaveAddFuzzDialog() : base("Add Fuzz...") {
            rand = new Random();
            fuzzAmt = new NumberBox("Amount", 8, 25, 94, 46, this);
            fuzzAmt.SetValueLimits(0, 100);
            fuzzAmt.DisplayMode = NumberBox.NumberDisplayMode.Percent;
            wrapAround = new CheckboxLabeled("Wrap values", 8, 40, 94, this);
            wrapAround.ShowCheckboxOnRight = true;

            newSeed = new Button("New seed", 8, 57, this);
            randomValues = new float[64];
            for (int i = 0; i < 64; ++i) {
                randomValues[i] = (float)rand.NextDouble() * 2 - 1;
            }
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                fuzzAmt.Update();
                wrapAround.Update();
                if (newSeed.Clicked) {
                    DoNewSeed();
                }
                if (fuzzAmt.ValueWasChangedInternally) {
                    Apply();
                }

                if (wrapAround.Clicked) {
                    Apply();
                }
            }
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        private void DoNewSeed() {
            for (int i = 0; i < 64; ++i) {
                randomValues[i] = (float)rand.NextDouble() * 2 - 1;
            }
            Apply();
        }

        protected override byte GetSampleValue(int index) {
            int samp = originalData[index];
            int sign = Math.Sign(randomValues[index]);
            for (int j = 0; j < Math.Abs((int)(randomValues[index] * fuzzAmt.Value / 100f * 32)); ++j) {
                if (samp + sign is > 31 or < 0) {
                    if (wrapAround.Value) {
                        sign *= -1;
                    }
                    else {
                        break;
                    }
                }
                samp += sign;
            }
            return (byte)samp;
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                fuzzAmt.Draw();
                wrapAround.Draw();
                newSeed.Draw();
            }
        }
    }
}
