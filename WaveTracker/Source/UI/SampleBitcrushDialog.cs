using System;

namespace WaveTracker.UI {
    public class SampleBitcrushDialog : SampleModifyDialog {
        private NumberBox downsample;
        private NumberBox bitDepth;
        private int resolutionfactor;

        public SampleBitcrushDialog() : base("Bitcrush...") {
            downsample = new NumberBox("Downsample", 8, 18, 92, 38, this);
            downsample.SetValueLimits(1, 128);
            bitDepth = new NumberBox("Bit depth", 8, 32, 92, 38, this);
            bitDepth.SetValueLimits(1, 16);
            downsample.Value = 1;
            bitDepth.Value = 16;
            resolutionfactor = 1;
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                downsample.Update();
                bitDepth.Update();
                if (bitDepth.ValueWasChangedInternally) {
                    resolutionfactor = (short)Math.Pow(2, 16 - bitDepth.Value);
                }
            }
        }

        protected override short GetSampleValue(int index, int channel) {
            index = index / downsample.Value * downsample.Value;
            if (channel == 0) {
                return (short)(Math.Floor((float)originalDataL[index] / resolutionfactor) * resolutionfactor + resolutionfactor / 2);
            }
            else {
                return (short)(Math.Floor((float)originalDataR[index] / resolutionfactor) * resolutionfactor + resolutionfactor / 2);
            }
        }

        public override void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                downsample.Draw();
                bitDepth.Draw();
            }
        }
    }
}
