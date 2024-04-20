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
    public class WaveAddFuzzDialog : Dialog {
        NumberBox fuzzAmt;
        CheckboxLabeled wrapAround;
        Wave waveToEdit;
        Button ok, cancel;
        Button newSeed;
        Random rand;
        float[] values;

        public WaveAddFuzzDialog() : base("Add Fuzz...", 258, 113, false) {
            rand = new Random();
            fuzzAmt = new NumberBox("Amount", 8, 25, 94, 46, this);
            fuzzAmt.SetValueLimits(0, 100);
            fuzzAmt.DisplayMode = NumberBox.NumberDisplayMode.Percent;
            wrapAround = new CheckboxLabeled("Wrap values", 8, 40, 94, this);
            wrapAround.ShowCheckboxOnRight = true;

            cancel = AddNewBottomButton("Cancel", this);
            ok = AddNewBottomButton("OK", this);
            newSeed = new Button("New seed", 8, 57, this);
            values = new float[64];
        }

        public override void Update() {
            if (windowIsOpen) {
                if (ok.Clicked) {
                    Close();
                    Apply();
                }
                if (cancel.Clicked) {
                    Close();
                }
                if (newSeed.Clicked) {
                    DoNewSeed();
                }
                fuzzAmt.Update();
                wrapAround.Update();
            }
        }

        public void Open(Wave wave) {
            waveToEdit = wave;
            DoNewSeed();
            Open();

        }

        void DoNewSeed() {
            for (int i = 0; i < 64; ++i) {
                values[i] = (float)rand.NextDouble() * 2 - 1;
            }
        }

        void Apply() {
            for (int i = 0; i < 64; ++i) {
                int samp = waveToEdit.GetSample(i);
                int sign = Math.Sign(values[i]);
                for (int j = 0; j < Math.Abs((int)(values[i] * fuzzAmt.Value / 100f * 32)); ++j) {
                    if (samp + sign > 31 || samp + sign < 0) {
                        if (wrapAround.Value)
                            sign *= -1;
                        else
                            break;
                    }
                    samp += sign;
                }
                waveToEdit.samples[i] = (byte)samp;
            }
        }

        public new void Draw() {
            if (windowIsOpen) {
                base.Draw();
                fuzzAmt.Draw();
                wrapAround.Draw();
                newSeed.Draw();

                Write("Parameters", 8, 15, UIColors.labelLight);
                Write("Preview", 121, 15, UIColors.labelLight);

                Color waveColor = new Color(200, 212, 93);
                Color waveBG = new Color(59, 125, 79, 150);
                Rectangle waveRegion = new Rectangle(121, 25, 128, 64);
                DrawRect(120, 24, 130, 66, UIColors.black);
                for (int i = 0; i < 64; ++i) {
                    int samp = waveToEdit.GetSample(i);
                    int sign = Math.Sign(values[i]);
                    for (int j = 0; j < Math.Abs((int)(values[i] * fuzzAmt.Value / 100f * 32)); ++j) {
                        if (samp + sign > 31 || samp + sign < 0) {
                            if (wrapAround.Value)
                                sign *= -1;
                            else
                                break;
                        }
                        samp += sign;
                    }
                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height / 2 + 1, 2, -2 * (samp - 16), waveBG);
                    DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height - samp * 2, 2, -2, waveColor);
                }
            }
        }
    }
}
