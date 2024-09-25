namespace WaveTracker.Tracker {
    public class NoiseInstrument : Instrument {
        public NoiseInstrument() : base() {
            name = "New Noise Instrument";
        }

        public override NoiseInstrument Clone() {
            NoiseInstrument m = new NoiseInstrument();
            m.name = name;
            m.envelopes = [];
            foreach (Envelope envelope in envelopes) {
                m.envelopes.Add(envelope.Clone());
            }
            return m;
        }
    }
}

