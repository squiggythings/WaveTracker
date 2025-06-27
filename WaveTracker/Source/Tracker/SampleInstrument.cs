using ProtoBuf;

namespace WaveTracker.Tracker {

    [ProtoContract]
    public class SampleInstrument : Instrument {
        [ProtoMember(21)]
        public Sample sample;

        public SampleInstrument() : base() {
            name = "New Sample Instrument";
            sample = new Sample();
        }

        public override SampleInstrument Clone() {
            SampleInstrument m = new SampleInstrument();
            m.name = name;
            m.envelopes = [];
            foreach (Envelope envelope in envelopes) {
                m.envelopes.Add(envelope.Clone());
            }
            m.sample.sampleDataL = new short[sample.sampleDataL.Length];
            m.sample.sampleDataR = new short[sample.sampleDataR.Length];
            for (int i = 0; i < sample.sampleDataL.Length; i++) {
                m.sample.sampleDataL[i] = sample.sampleDataL[i];
                if (sample.sampleDataR.Length != 0) {
                    m.sample.sampleDataR[i] = sample.sampleDataR[i];
                }
            }

            m.sample.loopType = sample.loopType;
            m.sample.loopPoint = sample.loopPoint;
            m.sample.resampleMode = sample.resampleMode;
            m.sample.SetBaseKey(sample.BaseKey);
            m.sample.SetDetune(sample.Detune);
            m.sample.useInVisualization = sample.useInVisualization;
            return m;
        }
    }
}
