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
            m.sample.sampleDataAccessL = new short[sample.sampleDataAccessL.Length];
            m.sample.sampleDataAccessR = new short[sample.sampleDataAccessR.Length];
            for (int i = 0; i < sample.sampleDataAccessL.Length; i++) {
                m.sample.sampleDataAccessL[i] = sample.sampleDataAccessL[i];
                if (sample.sampleDataAccessR.Length != 0) {
                    m.sample.sampleDataAccessR[i] = sample.sampleDataAccessR[i];
                }
            }

            m.sample.loopType = sample.loopType;
            m.sample.loopPoint = sample.loopPoint;
            m.sample.SetBaseKey(sample.BaseKey);
            m.sample.SetDetune(sample.Detune);
            m.sample.useInVisualization = sample.useInVisualization;
            return m;
        }
    }
}
