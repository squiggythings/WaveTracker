using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.Source {
    public class EvaluationContext {
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

    public class MathExpression {
        private Func<EvaluationContext, double> func;
        private string _expression;
        public string Expression {
            get {
                return _expression; 
            }
            set { 
                _expression = value; 
                RebuildExpression(); 
            }
        }
        public bool WaveFold = false;

        public MathExpression() {
            Expression = "0";
        }

        public MathExpression(string expression) {
            Expression = expression;
        }

        private void RebuildExpression() {
            Expression expression = new Expression(Expression);
            if (expression.HasErrors()) {
                throw expression.Error;
            }
            func = expression.ToLambda<EvaluationContext, double>();
        }

        public MathExpression Clone() {
            MathExpression newMathExpression = new MathExpression(Expression);
            newMathExpression.WaveFold = WaveFold;
            return newMathExpression;
        }
        public void Apply(Wave wave) {
            for (int i = 0; i < 64; ++i) {
                wave.samples[i] = GetSampleValue(i, wave.samples.Length);
            }
        }
        public byte GetSampleValue(int index, int length) {
            EvaluationContext context = new() {
                x = (index << 1) * Math.PI / length
            };
            return NormalizeExpressionOutput(func.Invoke(context), WaveFold);
        }
        public static byte NormalizeExpressionOutput(double d, bool fold = false) {
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
    }
}
