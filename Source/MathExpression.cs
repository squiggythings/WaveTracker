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
        //NCalc can't find const fields so the next best option was to make them private
#pragma warning disable CS0414 // The field is assigned but its value is never used
#pragma warning disable IDE0051 // Remove unused private members
        private double pi = Math.PI;
        private double e = Math.E;
        private double tau = Math.Tau;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore CS0414 // The field is assigned but its value is never used

        public double x; //"wave radian" maps the waves domain (0-64) to (0-2pi)

        //NCalc can't find static functions so they must be left as member functions
#pragma warning disable CA1822 // Mark members as static
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
#pragma warning restore CA1822 // Mark members as static
    }

    [ProtoContract(SkipConstructor = true)]
    [Serializable]
    public class MathExpression {
        [ProtoIgnore]
        private Func<EvaluationContext, double> func;

        [ProtoMember(1)]
        private string _expression;
        [ProtoMember(2)]
        public string Expression {
            get {
                return _expression; 
            }
            set { 
                _expression = value; 
                RebuildExpression(); 
            }
        }
        [ProtoMember(3)]
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
