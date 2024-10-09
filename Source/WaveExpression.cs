using NCalc;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveTracker.Tracker;

namespace WaveTracker.Source {
#pragma warning disable CA1822 // Mark members as static (NCalc can't find static functions)
#pragma warning disable CS0414 // The field is assigned but its value is never used (NCalc can't find const fields)
#pragma warning disable IDE0051 // Remove unused private members (NCalc can't find const fields)
    public class EvaluationContext {
        private double pi = Math.PI;
        private double e = Math.E;
        private double tau = Math.Tau;

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
    }
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore CS0414 // The field is assigned but its value is never used
#pragma warning restore CA1822 // Mark members as static

    [ProtoContract(SkipConstructor = true)]
    [Serializable]
    public class WaveExpression {
        [ProtoIgnore]
        private Func<EvaluationContext, double> func;

        private string _expression;
        [ProtoMember(1)]
        public string Expression {
            get {
                return _expression; 
            }
            set { 
                _expression = value; 
                RebuildExpression(); 
            }
        }
        [ProtoMember(2)]
        public bool WaveFold = false;

        public WaveExpression() {
            Expression = "0";
        }

        public WaveExpression(string expression) {
            Expression = expression;
        }

        private void RebuildExpression() {
            Expression expression = new Expression(Expression);
            if (expression.HasErrors()) {
                throw expression.Error;
            }
            func = expression.ToLambda<EvaluationContext, double>();
        }

        public WaveExpression Clone() {
            WaveExpression newMathExpression = new WaveExpression(Expression);
            newMathExpression.WaveFold = WaveFold;
            return newMathExpression;
        }

        public void Apply(Wave wave, EvaluationContext context) {
            for (int i = 0; i < 64; ++i) {
                wave.samples[i] = GetSampleValue(context);
            }
        }

        public byte GetSampleValue(EvaluationContext context) {
            
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
