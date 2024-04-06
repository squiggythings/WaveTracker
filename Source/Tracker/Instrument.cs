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
    [ProtoContract]
    [ProtoInclude(10, typeof(WaveInstrument))]
    [ProtoInclude(20, typeof(SampleInstrument))]
    public abstract class Instrument {
        [ProtoMember(1)]
        public string name;
        [ProtoMember(2)]
        public List<Envelope> envelopes;

        //public Envelope volumeEnvelope => envelopes[0];
        //public Envelope arpEnvelope => envelopes[1];
        //public Envelope pitchEnvelope => envelopes[2];


        //[ProtoMember(25)]
        //public int waveModType;

        

        public Instrument() {
            name = "New Instrument";
            envelopes = new List<Envelope>();
            envelopes.Add(new Envelope(Envelope.EnvelopeType.Volume));
            envelopes.Add(new Envelope(Envelope.EnvelopeType.Arpeggio));
            envelopes.Add(new Envelope(Envelope.EnvelopeType.Pitch));
            
            //volumeEnvelope = new Envelope(Envelope.EnvelopeType.Volume);
            //arpEnvelope = new Envelope(Envelope.EnvelopeType.Arpeggio);
            //pitchEnvelope = new Envelope(Envelope.EnvelopeType.Pitch);
            //waveEnvelope = new Envelope(Envelope.EnvelopeType.Wave);
            //waveModEnvelope = new Envelope(0);
           // waveModType = 0;

        }


        /*public bool IsEqualTo(Instrument other) {
            if (name != other.name)
                return false;

            if (instrumentType != other.instrumentType)
                return false;

            if (!volumeEnvelope.IsEqualTo(other.volumeEnvelope))
                return false;
            if (!arpEnvelope.IsEqualTo(other.arpEnvelope))
                return false;
            if (!pitchEnvelope.IsEqualTo(other.pitchEnvelope))
                return false;
            if (!waveEnvelope.IsEqualTo(other.waveEnvelope))
                return false;
            if (sample.sampleDataAccessL.Length != other.sample.sampleDataAccessL.Length)
                return false;
            if (sample.sampleLoopType != other.sample.sampleLoopType)
                return false;
            if (sample.sampleLoopIndex != other.sample.sampleLoopIndex)
                return false;
            if (sample.BaseKey != other.sample.BaseKey)
                return false;
            if (sample.Detune != other.sample.Detune)
                return false;
            if (sample.useInVisualization != other.sample.useInVisualization)
                return false;
            return true;
        }*/
        public abstract Instrument Clone();
        public override string ToString() {
            return name;
        }
        public void SetName(string name) {
            this.name = name;
        }
    }
}
