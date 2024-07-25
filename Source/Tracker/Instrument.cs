using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;
using System.Diagnostics;
using ProtoBuf;
using WaveTracker.Audio;

namespace WaveTracker.Tracker {
    /// <summary>
    /// Base class for an instrument
    /// </summary>
    [ProtoContract]
    [ProtoInclude(10, typeof(WaveInstrument))]
    [ProtoInclude(20, typeof(SampleInstrument))]
    public abstract class Instrument {
        [ProtoMember(1)]
        public string name;
        [ProtoMember(2)]
        public List<Envelope> envelopes;

        public Instrument() {
            name = "New Instrument";
            envelopes = new List<Envelope>();

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
