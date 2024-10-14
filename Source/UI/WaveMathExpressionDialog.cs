using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using WaveTracker.Tracker;
using WaveTracker.Source;

namespace WaveTracker.UI {
    public class WaveMathExpressionDialog : WaveModifyDialog {
        public Textbox expression;
        public CheckboxLabeled waveFold;

        private MathExpression mathExpression;
        private bool compileSuccess = true;
        private string lastCompileError = string.Empty;

        public WaveMathExpressionDialog() : base("Generate from maths expression...", 300) {
            expression = new Textbox("", 8, 25, 145, this);
            expression.Text = "0";
            expression.InputField.UpdateLive = true;

            waveFold = new CheckboxLabeled("Wave folding", 7, 42, 40, this);
            waveFold.SetTooltip("", "Wraps the waveform");
            waveFold.Value = false;

            mathExpression = new(expression.Text);
            expression.Update(); //Unsets ValueWasChanged flag
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                expression.Update();
                waveFold.Update();

                if (expression.ValueWasChanged) {
                    compileSuccess = true; //Set to true as a catch-all
                    try {
                        mathExpression.Expression = expression.Text;
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
                if (waveFold.Clicked) {
                    mathExpression.WaveFold = waveFold.Value;
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
                expression.Draw();
                waveFold.Draw();

                if (!compileSuccess) {
                    WriteMultiline("Invalid expression!", 8, 59, 145, Color.DarkRed);
                }
            }
        }
    }
}
