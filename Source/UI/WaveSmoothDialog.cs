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
    public class WaveSmoothDialog : WaveModifyDialog {
        NumberBox smoothWindow;
        NumberBox smoothAmt;

        public WaveSmoothDialog() : base("Smooth wave...") {
            smoothWindow = new NumberBox("Window size", 8, 25, 94, 40, this);
            smoothAmt = new NumberBox("Amount", 8, 40, 94, 40, this);
            smoothWindow.SetValueLimits(1, 8);
            smoothAmt.SetValueLimits(0, 10);
            smoothWindow.Value = 1;
            smoothAmt.Value = 0;
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                smoothWindow.Update();
                smoothAmt.Update();
                if (smoothAmt.ValueWasChangedInternally)
                    Apply();
                if (smoothWindow.ValueWasChangedInternally)
                    Apply();
            }
        }

        protected override byte GetSampleValue(int index) {
            byte[] samples = new byte[64];
            for (int i = 0; i < samples.Length; i++) {
                samples[i] = originalData[i];
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
            if (WindowIsOpen) {
                base.Draw();
                smoothWindow.Draw();
                smoothAmt.Draw();
            }
        }
    }
}
