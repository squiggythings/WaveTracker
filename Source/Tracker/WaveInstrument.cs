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
