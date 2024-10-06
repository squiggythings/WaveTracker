using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveSampleAndHoldDialog : WaveModifyDialog {
        private NumberBox holdLength;

        public WaveSampleAndHoldDialog() : base("Sample and hold...") {
            holdLength = new NumberBox("Hold length", 8, 25, 94, 36, this);
            holdLength.SetValueLimits(1, 32);
            holdLength.Value = 1;
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                holdLength.Update();
                if (holdLength.ValueWasChangedInternally) {
                    Apply();
                }
            }
        }

        protected override byte GetSampleValue(int index) {
            return originalData[index / holdLength.Value * holdLength.Value];
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                holdLength.Draw();
            }
        }
    }
}
