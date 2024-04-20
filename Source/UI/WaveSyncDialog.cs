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
    public class WaveSyncDialog : Dialog {
        Wave waveToEdit;
        NumberBox harmonicNum;
        Button ok, cancel;

        public WaveSyncDialog() : base("Set Harmonic...", 258, 113, false) {
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            harmonicNum = new NumberBox("Harmonic", 8, 25, 94, 40, this);
            harmonicNum.SetValueLimits(1, 32);

        }

        public void Open(Wave wave) {
            waveToEdit = wave;
            harmonicNum.Value = 1;
            Open();
        }

        public override void Update() {
            if (ok.Clicked) {
                Close();
                waveToEdit.Sync(harmonicNum.Value);
            }
            if (cancel.Clicked) {
                Close();
            }
            harmonicNum.Update();
        }

        int GetSampleValue(int index) {
            return waveToEdit.GetSample(index * harmonicNum.Value);
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                harmonicNum.Draw();

                Write("Parameters", 8, 15, UIColors.labelLight);
                Write("Preview", 121, 15, UIColors.labelLight);

                Color waveColor = new Color(200, 212, 93);
                Color waveBG = new Color(59, 125, 79, 150);
                Rectangle waveRegion = new Rectangle(121, 25, 128, 64);
                DrawRect(120, 24, 130, 66, UIColors.black);
                for (int i = 0; i < 64; ++i) {
                    int samp = GetSampleValue(i);

                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height / 2 + 1, 2, -2 * (samp - 16), waveBG);
                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height - samp * 2, 2, -2, waveColor);
                }
            }
        }
    }
}
