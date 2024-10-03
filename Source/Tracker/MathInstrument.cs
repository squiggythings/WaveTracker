using NCalc;
using ProtoBuf;
using System;
using WaveTracker.Source;

namespace WaveTracker.Tracker {

    [ProtoContract]
    public class MathInstrument : Instrument {
        [ProtoMember(41)]
        public MathExpression MathExpression { get; set; }

        [ProtoMember(42)]
        public Wave MathWave { get; private set; }

        public MathInstrument() : base() {
            name = "New Math Instrument";
            MathWave = new Wave();
            MathExpression = new MathExpression("sin(x)");
            MathExpression.Apply(MathWave);
        }

        public override MathInstrument Clone() {
            MathInstrument m = new MathInstrument();
            m.name = name;
            m.envelopes = [];
            foreach (Envelope envelope in envelopes) {
                m.envelopes.Add(envelope.Clone());
            }
            m.MathExpression = MathExpression.Clone();
            return m;
        }
    }
}
