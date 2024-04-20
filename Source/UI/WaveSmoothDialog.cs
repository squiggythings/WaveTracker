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
    public class WaveSmoothDialog : Dialog {
        Wave waveToEdit;
        NumberBox smoothWindow;
        NumberBox smoothAmt;

        Button ok, cancel;

        public WaveSmoothDialog() : base("Smooth wave...", 258, 113, false) {
            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            smoothWindow = new NumberBox("Window size", 8, 25, 94, 40, this);
            smoothAmt = new NumberBox("Amount", 8, 40, 94, 40, this);
            smoothWindow.SetValueLimits(1, 8);
            smoothAmt.SetValueLimits(0, 10);


        }

        public void Open(Wave wave) {
            waveToEdit = wave;
            smoothWindow.Value = 1;
            smoothAmt.Value = 0;
            Open();
        }

        public override void Update() {
            if (ok.Clicked) {
                Close();
                waveToEdit.Smooth(smoothWindow.Value, smoothAmt.Value);
            }
            if (cancel.Clicked) {
                Close();
            }
            smoothWindow.Update();
            smoothAmt.Update();
        }

        int GetSampleValue(int index) {
            byte[] samples = new byte[64];
            for (int i = 0; i < samples.Length; i++) {
                samples[i] = waveToEdit.samples[i];
            }
            for (int a = 0; a < smoothAmt.Value; a++) {
                byte[] ret = new byte[64];
                for (int i = 0; i < 64; ++i) {
                    int sum = 0;
                    for (int j = -smoothWindow.Value; j <= smoothWindow.Value; j++) {
                        sum += samples[(j + i + 128) % 64];
                    }
                    ret[i] = (byte)Math.Round(sum / (smoothWindow.Value * 2 + 1f));
                }
                for (int i = 0; i < ret.Length; i++) {
                    samples[i] = ret[i];
                }
            }
            return samples[index];
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                smoothWindow.Draw();
                smoothAmt.Draw();

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
