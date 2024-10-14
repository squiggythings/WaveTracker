using NCalc;
using ProtoBuf;
using System;
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

        public double x;
        public double t;

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

        public double Sin(double x) {
            return Math.Sin(x * 2.0 * Math.PI);
        }
        public double Sine(double x) {
            return Math.Sin(x * 2.0 * Math.PI);
        }
        public double Cos(double x) {
            return Math.Cos(x * 2.0 * Math.PI);
        }
        public double Cosine(double x) {
            return Math.Cos(x * 2.0 * Math.PI);
        }
        public double Sqr(double x) {
            return (Mod(x, 1) < 0.5) ? -1 : 1;
        }
        public double Square(double x) {
            return (Mod(x, 1) < 0.5) ? -1 : 1;
        }
        public double Pulse(double x, double width) {
            return (Mod(x, 1) < width) ? -1 : 1;
        }
        public double Saw(double x) {
            return Mod(x + 0.5, 1) * 2 - 1;
        }
        public double Sawtooth(double x) {
            return Mod(x + 0.5, 1) * 2 - 1;
        }
        public double Tri(double x) {
            return Math.Abs(Mod(2 * x - 0.5, 2) * 2 - 2) - 1;
        }
        public double Triangle(double x) {
            return Math.Abs(Mod(2 * x - 0.5, 2) * 2 - 2) - 1;
        }
    }
#pragma warning restore CA1822 // Mark members as static

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
                x = (double)index / length,
                t = (double)index / length
            };
            return NormalizeExpressionOutput(func.Invoke(context), WaveFold);
        }
        public static byte NormalizeExpressionOutput(double d, bool fold = false) {
            if (fold) {
                // Thanks to https://www.kvraudio.com/forum/viewtopic.php?t=501471 for the dFolded code
                double dFolded = 4.0 * (Math.Abs(0.25 * d + 0.25 - Math.Round(0.25 * d + 0.25)) - 0.25);

                return Math.Clamp((byte)Helpers.MapClamped((float)dFolded, -1, 1, Wave.MinSampleValue, Wave.MaxSampleValue + 1), Wave.MinSampleValue, Wave.MaxSampleValue);
            }
            else {
                //double dScaled = Math.Floor(d * (Wave.MaxSampleValue / 2f));
                return Math.Clamp((byte)Helpers.MapClamped((float)d, -1, 1, Wave.MinSampleValue, Wave.MaxSampleValue + 1), Wave.MinSampleValue, Wave.MaxSampleValue);
            }
        }
    }
}
