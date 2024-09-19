using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

