using Microsoft.Xna.Framework;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public abstract class WaveModifyDialog : Dialog {
        protected Wave waveToEdit;
        protected byte[] originalData;
        private Button ok, cancel;
        protected Point previewAnchor;

        public WaveModifyDialog(string name) : base(name, 258, 113, false) {
            previewAnchor = new Point(258 - 137, 15);
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
        }

        public WaveModifyDialog(string name, int width) : base(name, width, 113, false) {
            previewAnchor = new Point(width - 137, 15);
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
        }

        public void Open(Wave wave) {
            waveToEdit = wave;
            originalData = new byte[64];
            for (int i = 0; i < originalData.Length; ++i) {
                originalData[i] = wave.samples[i];
            }
            Open();
            Apply();
        }

        public override void Update() {
            if (WindowIsOpen) {
                if (ok.Clicked) {
                    Apply();
                    Close();
                }
                if (cancel.Clicked) {
                    Close();
                    Revert();
                }
            }
        }

        /// <summary>
        /// Applies the effect to the wave
        /// </summary>
        public void Apply() {
            if (waveToEdit != null) {
                for (int i = 0; i < 64; ++i) {
                    waveToEdit.samples[i] = GetSampleValue(i);
                }
            }
        }

        public void Revert() {
            for (int i = 0; i < originalData.Length; ++i) {
                waveToEdit.samples[i] = originalData[i];
            }
        }

        /// <summary>
        /// Given the input index, define the output sample
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract byte GetSampleValue(int index);

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();

                Write("Parameters", 8, 15, UIColors.labelLight);
                Write("Preview", previewAnchor.X, previewAnchor.Y, UIColors.labelLight);

                Color waveColor = new Color(200, 212, 93);
                Color waveBG = new Color(59, 125, 79, 150);
                Rectangle waveRegion = new Rectangle(previewAnchor.X, previewAnchor.Y + 10, 128, 64);
                DrawRect(previewAnchor.X - 1, previewAnchor.Y + 9, 130, 66, UIColors.black);
                for (int i = 0; i < 64; ++i) {
                    int samp = waveToEdit.GetSample(i);

                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height / 2 + 1, 2, -2 * (samp - 16), waveBG);
                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height - samp * 2, 2, -2, waveColor);
                }
            }
        }
    }
}
