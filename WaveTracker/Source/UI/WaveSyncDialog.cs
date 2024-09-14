using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveSyncDialog : WaveModifyDialog {
        private NumberBox harmonicNum;

        public WaveSyncDialog() : base("Set Harmonic...") {
            harmonicNum = new NumberBox("Harmonic", 8, 25, 94, 40, this);
            harmonicNum.SetValueLimits(1, 32);
            harmonicNum.Value = 1;
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                harmonicNum.Update();
                if (harmonicNum.ValueWasChangedInternally) {
                    Apply();
                }
            }
        }

        protected override byte GetSampleValue(int index) {
            return originalData[index * harmonicNum.Value % 64];
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                harmonicNum.Draw();
            }
        }
    }
}
