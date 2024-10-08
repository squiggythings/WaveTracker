using NAudio.MediaFoundation;
using ProtoBuf;
using System.Collections.Generic;

namespace WaveTracker.Tracker {
    /// <summary>
    /// Base class for an instrument
    /// </summary>
    [ProtoContract]
    [ProtoInclude(10, typeof(WaveInstrument))]
    [ProtoInclude(20, typeof(SampleInstrument))]
    [ProtoInclude(30, typeof(NoiseInstrument))]
    public abstract class Instrument {
        [ProtoMember(1)]
        public string name;
        [ProtoMember(2)]
        public List<Envelope> envelopes;

        public Instrument() {
            name = "New Instrument";
            envelopes = [];

        }

        public bool HasEnvelope(Envelope.EnvelopeType type) {
            foreach (Envelope envelope in envelopes) {
                if (envelope.Type == type) {
                    return true;
                }
            }
            return false;
        }

        public abstract Instrument Clone();
        public override string ToString() {
            return name;
        }
        public void SetName(string name) {
            this.name = name;
        }
    }
}
