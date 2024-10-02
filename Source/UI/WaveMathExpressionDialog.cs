using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Forms;
using WaveTracker.Source;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveMathExpressionDialog : WaveModifyDialog {
        public Textbox MathExpressionInput;
        public CheckboxLabeled WaveFoldCheckbox;
        private List<string> compiledExpression = ["0"];

        private double parseTime = 0;
        private bool exprParseSuccess = true;
        private string lastParseError = string.Empty;

        public WaveMathExpressionDialog() : base("Generate from maths expression...", 300) {
            MathExpressionInput = new Textbox("", 8, 25, 145, this);
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
                        Stopwatch sw = Stopwatch.StartNew();
                        compiledExpression = ExpressionParser.CompileInfixToRPN(MathExpressionInput.Text);
                        Apply();
                        parseTime = sw.Elapsed.TotalMilliseconds;
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
            ExpressionParser.symbols["x"] = sampleRadian;

            return NormalizeExpressionOutput(
                    ExpressionParser.EvaluateRPNTokens(compiledExpression),
                    WaveFoldCheckbox.Value);
        }

        /// <summary>
        /// Maps the range of sin(t) [-1, 1] -> [0, 31]
        /// </summary>
        private  static byte NormalizeExpressionOutput(double d, bool fold = false) {
            if (fold) {
                // Thanks to https://www.kvraudio.com/forum/viewtopic.php?t=501471 for the dFolded code
                double dFolded = 4.0 * (Math.Abs(0.25 * d + 0.25 - Math.Round(0.25 * d + 0.25)) - 0.25);
                double dScaled = Math.Round((dFolded + 1) * (Wave.MaxSampleValue / 2f));

                return (byte)dScaled;
            }
            else {
                double dScaled = Math.Round((d + 1) * (Wave.MaxSampleValue / 2f));
                return (byte)Math.Clamp(
                dScaled,
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
                    Write($"Compilation successful ({Math.Round(parseTime, 3)} ms)", 8, 59, Color.Green);
                }
                else {
                    WriteMultiline("Compilation failed: " + lastParseError, 8, 59, 145, Color.OrangeRed);
                }
            }
        }
    }
}
