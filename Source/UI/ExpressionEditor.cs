using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Xml.XPath;
using WaveTracker.Audio;
using WaveTracker.Source;
using WaveTracker.Tracker;

namespace WaveTracker.UI {
    public class ExpressionEditor : Element {
        /// <summary>
        /// The sample to edit
        /// </summary>
        public MathExpression MathExpression { get; set; }
        public Wave MathWave { get; set; }

        public Textbox MathExpressionInput;
        public CheckboxLabeled WaveFoldCheckbox;

        private double compileTime = 0;
        private bool compileSuccess = true;
        private string lastCompileError = string.Empty;

        protected Point previewAnchor;

        public ExpressionEditor(int x, int y, Element parent) {
            this.x = x;
            this.y = y;
            previewAnchor = new Point(300 - 137, 15);

            SetParent(parent);
            MathExpressionInput = new Textbox("", 8, 25, 145, this);
            MathExpressionInput.Text = "0";

            WaveFoldCheckbox = new CheckboxLabeled("Wave folding", 7, 42, 40, this);
            WaveFoldCheckbox.SetTooltip("", "Wraps the waveform");
            WaveFoldCheckbox.Value = false;

            MathExpression = new(MathExpressionInput.Text);
            MathExpressionInput.Update(); //Unsets ValueWasChanged flag
        }

        public void Update() {
            MathExpressionInput.Update();
            WaveFoldCheckbox.Update();

            if (MathExpressionInput.ValueWasChanged) {
                compileSuccess = true; //Set to true as a catch-all
                try {
                    Stopwatch sw = Stopwatch.StartNew();
                    MathExpression.Expression = MathExpressionInput.Text;
                    compileTime = sw.Elapsed.TotalMilliseconds;

                    MathExpression.Apply(MathWave);
                } catch (Exception e) {
                    compileSuccess = false;
                    if (e.InnerException != null) {
                        lastCompileError = e.InnerException.Message;
                    }
                    else {
                        lastCompileError = e.Message;
                    }
                }
            }
            if (WaveFoldCheckbox.Clicked) {
                MathExpression.WaveFold = WaveFoldCheckbox.Value;
                MathExpression.Apply(MathWave);
            }
        }

        public void Draw() {
            MathExpressionInput.Draw();
            WaveFoldCheckbox.Draw();

            if (compileSuccess) {
                Write($"Compilation successful ({Math.Round(compileTime, 3)} ms)", 8, 59, Color.Green);
            }
            else {
                WriteMultiline("Compilation failed: " + lastCompileError, 8, 59, 145, Color.OrangeRed);
            }

            Write("Parameters", 8, 15, UIColors.labelLight);
            Write("Preview", previewAnchor.X, previewAnchor.Y, UIColors.labelLight);

            Color waveColor = new Color(200, 212, 93);
            Color waveBG = new Color(59, 125, 79, 150);
            Rectangle waveRegion = new Rectangle(previewAnchor.X, previewAnchor.Y + 10, 128, 64);
            DrawRect(previewAnchor.X - 1, previewAnchor.Y + 9, 130, 66, UIColors.black);
            for (int i = 0; i < 64; ++i) {
                int samp = MathWave.GetSample(i);

                DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height / 2 + 1, 2, -2 * (samp - 16), waveBG);
                DrawRect(waveRegion.X + i * 2, waveRegion.Y + waveRegion.Height - samp * 2, 2, -2, waveColor);
            }
        }
        private static int GetXPositionOfSample(int sampleIndex, int dataLength, int maxWidth) {
            return (int)((float)sampleIndex / dataLength * maxWidth);
        }
    }
}
