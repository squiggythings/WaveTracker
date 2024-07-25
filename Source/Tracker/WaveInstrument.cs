using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;



namespace WaveTracker.Tracker {
    public class WaveInstrument : Instrument {

        public WaveInstrument() : base() {
            name = "New Wave Instrument";
            //envelopes.Add(new Envelope(Envelope.EnvelopeType.Wave));
        }

        /// <summary>
        /// Adds an envelope of type <c>type</c> if it does not already exist in this instrument
        /// </summary>
        /// <param name="type"></param>
        public void AddEnvelope(Envelope.EnvelopeType type) {
            foreach (Envelope envelope in envelopes) {
                if (envelope.Type == type) {
                    return;
                }
            }
            envelopes.Add(new Envelope(type));
        }
        /// <summary>
        /// Removes an envelope of type <c>type</c> if it already exists in this instrument
        /// </summary>
        /// <param name="type"></param>
        public void RemoveEnvelope(Envelope.EnvelopeType type) {
            for (int i = 0; i < envelopes.Count; i++) {
                if (envelopes[i].Type == type) {
                    envelopes.RemoveAt(i);
                    return;
                }
            }
        }

        public override WaveInstrument Clone() {
            WaveInstrument m = new WaveInstrument();
            m.name = name;
            m.envelopes = new List<Envelope>();
            foreach (Envelope envelope in envelopes) {
                m.envelopes.Add(envelope.Clone());
            }
            return m;
        }
    }
}
