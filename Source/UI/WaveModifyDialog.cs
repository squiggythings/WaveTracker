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
    public abstract class WaveModifyDialog : Dialog {
        protected Wave waveToEdit;
        protected byte[] originalData;
        Button ok, cancel;
        public WaveModifyDialog(string name) : base(name, 258, 113, false) {
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
        }

        

        public override void Update() {
            if (windowIsOpen) {
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

        public void Apply() {
            for (int i = 0; i < 64; ++i) {
                waveToEdit.samples[i] = GetSampleValue(i);
            }
        }

        public void Revert() {
            for (int i = 0; i < originalData.Length; ++i) {
                waveToEdit.samples[i] = originalData[i];
            }
        }
        protected abstract byte GetSampleValue(int index);

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();

                Write("Parameters", 8, 15, UIColors.labelLight);
                Write("Preview", 121, 15, UIColors.labelLight);

                Color waveColor = new Color(200, 212, 93);
                Color waveBG = new Color(59, 125, 79, 150);
                Rectangle waveRegion = new Rectangle(121, 25, 128, 64);
                DrawRect(120, 24, 130, 66, UIColors.black);
                for (int i = 0; i < 64; ++i) {
                    int samp = waveToEdit.GetSample(i);

                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height / 2 + 1, 2, -2 * (samp - 16), waveBG);
                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height - samp * 2, 2, -2, waveColor);
                }
            }
        }
    }
}
