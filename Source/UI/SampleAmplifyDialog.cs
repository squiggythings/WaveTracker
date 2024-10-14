using System;

namespace WaveTracker.UI {
    public class SampleAmplifyDialog : SampleModifyDialog {
        private NumberBox amplifyAmount;
        private HorizontalSlider amplifySlider;
        private float amplifyFactor;

        public SampleAmplifyDialog() : base("Amplify...") {
            amplifyAmount = new NumberBox("Amplification (db)", 8, 15, this);
            amplifyAmount.SetValueLimits(-50, 50);
            amplifySlider = new HorizontalSlider(8, 35, width - 16, 12, this);
            amplifySlider.SetValueLimits(-50, 50);
            amplifyFactor = MathF.Pow(10, amplifyAmount.Value / 10f);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                amplifyAmount.Update();
                if (amplifyAmount.ValueWasChangedInternally) {
                    amplifySlider.Value = amplifyAmount.Value;
                }
                amplifySlider.Update();
                amplifyAmount.Value = amplifySlider.Value;
                amplifyFactor = MathF.Pow(10, amplifyAmount.Value / 10f);

            }
        }

        protected override short GetSampleValue(int index, int channel) {
            if (channel == 0) {
                return (short)Math.Clamp(originalDataL[index] * amplifyFactor, short.MinValue, short.MaxValue);
            }
            else {
                return (short)Math.Clamp(originalDataR[index] * amplifyFactor, short.MinValue, short.MaxValue);
            }
        }

        public override void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                amplifySlider.Draw();
                amplifyAmount.Draw();
            }
        }
    }
}
