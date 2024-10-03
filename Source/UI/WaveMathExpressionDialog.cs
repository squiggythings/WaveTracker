using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NCalc;
using System.Windows.Forms;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class WaveMathExpressionDialog : WaveModifyDialog {
        internal class EvaluationContext {
            //These values are used by NCalc but have been made private as const members don't work
#pragma warning disable CS0414 // The field is assigned but its value is never used
#pragma warning disable IDE0051 // Remove unused private members
            private double pi = Math.PI;
            private double e = Math.E;
            private double tau = Math.Tau;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore CS0414 // The field is assigned but its value is never used

            public double x; //"wave radian" maps the waves domain (0-64) to (0-2pi)
            public double ticks = App.CurrentModule.TickRate;

        }

        public Textbox MathExpressionInput;
        public CheckboxLabeled WaveFoldCheckbox;

        private Func<EvaluationContext, double> compiledExpression = (_) => { return 0; };
        private double compileTime = 0;
        private bool compileSuccess = true;
        private string lastCompileError = string.Empty;

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

                if (MathExpressionInput.ValueWasChanged) {
                    compileSuccess = true; //Set to true as a catch-all
                    try {
                        Stopwatch sw = Stopwatch.StartNew();

                        Expression expression = new Expression(MathExpressionInput.Text);
                        if (expression.HasErrors()) {
                            throw expression.Error;
                        }
                        compiledExpression = expression.ToLambda<EvaluationContext, double>();

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
                    Apply();
                }
            }
        }

        protected override byte GetSampleValue(int index) {
            EvaluationContext context = new() {
                x = (index << 1) * Math.PI / originalData.Length
            };
            return NormalizeExpressionOutput(compiledExpression.Invoke(context));
        }

        /// <summary>
        /// Maps the range of sin(x) [-1, 1] -> [0, 31]
        /// </summary>
        private  static byte NormalizeExpressionOutput(double d, bool fold = false) {
            if (fold) {
                // Thanks to https://www.kvraudio.com/forum/viewtopic.php?t=501471 for the dFolded code
                double dFolded = 4.0 * (Math.Abs(0.25 * d + 0.25 - Math.Round(0.25 * d + 0.25)) - 0.25);
                double dScaled = Math.Round((dFolded + 1) * (Wave.MaxSampleValue / 2f));

                return (byte)dScaled;
            }
            else {
                double dScaled = Math.Round((d + 1) * (Wave.MaxSampleValue / 2f), MidpointRounding.AwayFromZero);
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
