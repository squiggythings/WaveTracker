using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using WaveTracker.Tracker;
using WaveTracker.Source;

namespace WaveTracker.UI {
    public class WaveExpressionDialog : WaveModifyDialog {
        private Textbox ExpressionInput;
        private CheckboxLabeled WaveFoldCheckbox;
        private NumberBoxDecimal InputA, InputB, InputC;
        private TextBlock compileMessage;

        private WaveExpression waveExpression;

        public WaveExpressionDialog() : base("Wave expression...", 340) {
            ExpressionInput = new Textbox("", 8, 25, 170, this);
            ExpressionInput.SetTooltip("", "Expression");
            ExpressionInput.Text = "sin(x)";

            waveExpression = new(ExpressionInput.Text);
            ExpressionInput.Update(); //Unsets ValueWasChanged flag

            int inputValueX = 8;
            InputA = new NumberBoxDecimal("A", inputValueX, 42, this);
            inputValueX += 60;
            InputB = new NumberBoxDecimal("B", inputValueX, 42, this);
            inputValueX += 60;
            InputC = new NumberBoxDecimal("C", inputValueX, 42, this);

            WaveFoldCheckbox = new CheckboxLabeled("Wave folding", 7, 59, 40, this);
            WaveFoldCheckbox.SetTooltip("", "Wraps the waveform");
            WaveFoldCheckbox.Value = false;

            compileMessage = new TextBlock("", 8, 74, 170, 38, this);
        }

        public new void Open(Wave wave) {
            base.Open(wave);
        }

        public override void Update() {
            if (WindowIsOpen) {
                base.Update();
                ExpressionInput.Update();
                WaveFoldCheckbox.Update();
                InputA.Update();
                InputB.Update();
                InputC.Update();
                compileMessage.Update();

                if (ExpressionInput.ValueWasChanged) {
                    try {
                        Stopwatch sw = Stopwatch.StartNew();

                        waveExpression.Expression = ExpressionInput.Text;

                        SetCompileMessage($"Compilation successful ({Math.Round(sw.Elapsed.TotalMilliseconds, 3)} ms)", Color.Green);
                        Apply();
                    } catch (Exception e) {
                        //In the case the expression cannot be parsed (exception),
                        //the previous expression will still be valid.
                        //This might be a little weird sofeel free to reset it here
                        /*mathExpression = new("sin(x)");
                        Apply();*/

                        //Somtimes NCalc hides the important error in an inner exception
                        string errorMessage;
                        if (e.InnerException != null) {
                            errorMessage = e.InnerException.Message;
                        }
                        else {
                            errorMessage = e.Message;
                        }

                        SetCompileMessage("Compilation failed: " + errorMessage, Color.OrangeRed);
                    }
                }
                if (WaveFoldCheckbox.Clicked) {
                    Apply();
                }
                if (InputA.ValueWasChanged || InputB.ValueWasChanged || InputC.ValueWasChanged) {
                    Apply();
                }
            }
        }

        protected override byte GetSampleValue(int index) {
            EvaluationContext context = new() {
                wavefold = WaveFoldCheckbox.Value,
                x = (index << 1) * Math.PI / waveToEdit.samples.Length,
                a = InputA.Value,
                b = InputB.Value,
                c = InputC.Value
            };
            return waveExpression.Evaluate(context);
        }

        public new void Draw() {
            if (WindowIsOpen) {
                base.Draw();
                ExpressionInput.Draw();
                WaveFoldCheckbox.Draw();
                InputA.Draw();
                InputB.Draw();
                InputC.Draw();
                compileMessage.Draw();
            }
        }

        private void SetCompileMessage(string message, Color textColor) {
            compileMessage.Text = message;
            compileMessage.TextColour = textColor;
        }
    }
}
