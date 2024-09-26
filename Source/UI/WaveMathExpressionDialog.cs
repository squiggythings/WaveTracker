using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WaveTracker.Source;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveMathExpressionDialog : WaveModifyDialog {
        public Textbox MathExpressionInput;
        public CheckboxLabeled WaveFoldCheckbox;
        private List<string> compiledExpression = [];

        private bool exprParseSuccess = true;
        private string lastParseError = string.Empty;

        public WaveMathExpressionDialog() : base("Generate from maths expression...") {
            MathExpressionInput = new Textbox("", 8, 25, 100, this);
            MathExpressionInput.Text = "0";

            WaveFoldCheckbox = new CheckboxLabeled("Wave folding", 7, 42, 40, this);
            WaveFoldCheckbox.SetTooltip("", "Wraps the waveform");
            WaveFoldCheckbox.Value = false;
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                MathExpressionInput.Update();
                WaveFoldCheckbox.Update();

                if (MathExpressionInput.ValueWasChanged || WaveFoldCheckbox.Clicked) {
                    exprParseSuccess = true; //Set to true as a catch-all
                    try {
                        compiledExpression = ExpressionParser.CompileInfixToRPN(MathExpressionInput.Text);
                        Apply();
                    } catch (Exception e) {
                        exprParseSuccess = false;
                        lastParseError = e.Message;
                    }
                }
            }
        }

        protected override byte GetSampleValue(int index) {
            //Applying expression
            double sampleRadian = (index << 1) * Math.PI / originalData.Length;
            try {
                return RemapExpressionOutputToByte(
                    ExpressionParser.EvaluateRPNTokens(compiledExpression, ("t", sampleRadian)),
                    WaveFoldCheckbox.Value);
            } catch (Exception e) {
                exprParseSuccess = false;
                lastParseError = e.Message;
                return waveToEdit.samples[index];
            }
        }

        /// <summary>
        /// Maps the range of sin(t) [-1, 1] -> [0, 31]
        /// </summary>
        private  static byte RemapExpressionOutputToByte(double d, bool fold = false) {
            double dValue = Math.Round((d + 1) * (Wave.MaxSampleValue / 2f));
            if (fold) {
                return (byte)(dValue % (Wave.MaxSampleValue + 1));
            }
            else {
                return (byte)Math.Clamp(
                dValue,
                Wave.MinSampleValue,
                Wave.MaxSampleValue);
            }
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                MathExpressionInput.Draw();
                WaveFoldCheckbox.Draw();

                if (exprParseSuccess) {
                    Write("Compilation successful", 8, 59, Color.Green);
                }
                else {
                    WriteMultiline("Compilation failed: " + lastParseError, 8, 59, 100, Color.OrangeRed);
                }
            }
        }
    }
}
