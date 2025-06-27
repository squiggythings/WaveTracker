namespace WaveTracker.Tracker {
    public class WaveInstrument : Instrument {

        public WaveInstrument() : base() {
            name = "New Wave Instrument";
        }

        public override WaveInstrument Clone() {
            WaveInstrument m = new WaveInstrument();
            m.name = name;
            m.envelopes = [];
            foreach (Envelope envelope in envelopes) {
                m.envelopes.Add(envelope.Clone());
            }
            return m;
        }
    }
}
