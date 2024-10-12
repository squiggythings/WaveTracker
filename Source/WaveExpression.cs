using NCalc;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.Source {
    public class EvaluationContext {
#pragma warning disable CA1822 // Mark members as static (NCalc can't find static functions)
#pragma warning disable CS0414 // The field is assigned but its value is never used (NCalc can't find const fields)
#pragma warning disable IDE0051 // Remove unused private members (NCalc can't find const fields)
        private double pi = Math.PI;
        private double e = Math.E;
        private double tau = Math.Tau;

        public bool wavefold = false;

        public double x = 0; //"wave radian" maps the waves domain (0-64) to (0-2pi)
        public double a = 0;
        public double b = 0;
        public double c = 0;

        public double Rand() {
            return Random.Shared.Next(-byte.MaxValue, byte.MaxValue + 1) / 255.0;
        }
        public double Randb() {
            return Random.Shared.Next(byte.MinValue, byte.MaxValue + 1);
        }
        public double Randuc() {
            return Random.Shared.Next();
        }
        public double Deg2rad(double d) {
            return Math.PI * d / 180.0;
        }
        public double Rad2deg(double r) {
            return r * (180.0 / Math.PI);
        }
        public double Mod(double a, double b) {
            return a - b * Math.Floor(a / b);
        }
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore CS0414 // The field is assigned but its value is never used
#pragma warning restore CA1822 // Mark members as static
    }

    public class WaveExpression {
        private Func<EvaluationContext, double> lambda;
        private string _expression;
        public string Expression {
            get {
                return _expression; 
            }
            set { 
                TryUpdateExpression(value); 
                _expression = value; 
            }
        }

        public WaveExpression() {
            Expression = "0";
        }

        public WaveExpression(string expression) {
            Expression = expression;
        }

        private void TryUpdateExpression(string str) {
            Expression expression = new Expression(str);
            if (expression.HasErrors()) {
                throw expression.Error;
            }
            lambda = expression.ToLambda<EvaluationContext, double>();
        }

        public void Apply(Wave wave, EvaluationContext context) {
            for (int i = 0; i < 64; ++i) {
                wave.samples[i] = Evaluate(context);
            }
        }

        public byte Evaluate(EvaluationContext context) {
            
            return NormalizeExpressionOutput(lambda.Invoke(context), context.wavefold);
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

        public WaveExpression Clone() {
            WaveExpression newMathExpression = new WaveExpression(Expression);
            return newMathExpression;
        }
    }
}
