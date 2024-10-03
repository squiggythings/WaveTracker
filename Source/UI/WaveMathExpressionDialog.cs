using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NCalc;
using System.Windows.Forms;
using WaveTracker.Tracker;
using WaveTracker.Source;

namespace WaveTracker.UI {
    public class WaveMathExpressionDialog : WaveModifyDialog {
        public Textbox MathExpressionInput;
        public CheckboxLabeled WaveFoldCheckbox;

        MathExpression mathExpression;
        private double compileTime = 0;
        private bool compileSuccess = true;
        private string lastCompileError = string.Empty;

        public WaveMathExpressionDialog() : base("Generate from maths expression...", 300) {
            MathExpressionInput = new Textbox("", 8, 25, 145, this);
            MathExpressionInput.Text = "0";

            WaveFoldCheckbox = new CheckboxLabeled("Wave folding", 7, 42, 40, this);
            WaveFoldCheckbox.SetTooltip("", "Wraps the waveform");
            WaveFoldCheckbox.Value = false;

            mathExpression = new(MathExpressionInput.Text);
            MathExpressionInput.Update(); //Unsets ValueWasChanged flag
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                MathExpressionInput.Update();
                WaveFoldCheckbox.Update();

                if (MathExpressionInput.ValueWasChanged) {
                    compileSuccess = true; //Set to true as a catch-all
                    try {
                        Stopwatch sw = Stopwatch.StartNew();

                        mathExpression.Expression = MathExpressionInput.Text;

                        compileTime = sw.Elapsed.TotalMilliseconds;
                        Apply();
                    } catch (Exception e) {
                        compileSuccess = false;
                        if(e.InnerException != null) {
                            lastCompileError = e.InnerException.Message;
                        }
                        else {
                            lastCompileError = e.Message;
                        }
                    }
                }
                if (WaveFoldCheckbox.Clicked) {
                    mathExpression.WaveFold = WaveFoldCheckbox.Value;
                    Apply();
                }
            }
        }

        protected override byte GetSampleValue(int index) {
            return mathExpression.GetSampleValue(index, waveToEdit.samples.Length);
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                MathExpressionInput.Draw();
                WaveFoldCheckbox.Draw();

                if (compileSuccess) {
                    Write($"Compilation successful ({Math.Round(compileTime, 3)} ms)", 8, 59, Color.Green);
                }
                else {
                    WriteMultiline("Compilation failed: " + lastCompileError, 8, 59, 145, Color.OrangeRed);
                }
            }
        }
    }
}
