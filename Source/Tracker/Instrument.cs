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
    [Serializable]
    public enum InstrumentType {
        Wave,
        Sample
    }
    [Serializable]
    [ProtoContract]
    public class Instrument {
        [ProtoMember(17)]
        public string name;
        [ProtoMember(18)]
        public InstrumentType instrumentType;
        [ProtoMember(19)]
        public Envelope volumeEnvelope;
        [ProtoMember(20)]
        public Envelope arpEnvelope;
        [ProtoMember(21)]
        public Envelope pitchEnvelope;
        [ProtoMember(22)]
        public Envelope waveEnvelope;
        [ProtoMember(24)]
        public Envelope waveModEnvelope;
        [ProtoMember(25)]
        public int waveModType;

        [ProtoMember(23)]
        public Sample sample;
        public const char delimiter = '%';

        public Instrument() {
            instrumentType = InstrumentType.Wave;
            name = "New Instrument";
            volumeEnvelope = new Envelope(99);
            arpEnvelope = new Envelope(0);
            pitchEnvelope = new Envelope(0);
            waveEnvelope = new Envelope(0);
            waveModEnvelope = new Envelope(0);
            waveModType = 0;

            sample = new Sample();
        }
        public Instrument(InstrumentType type) {
            instrumentType = type;
            if (type == InstrumentType.Wave)
                name = "New Wave Instrument";
            else if (type == InstrumentType.Sample)
                name = "New Sample Instrument";
            else
                name = "New Instrument";
            volumeEnvelope = new Envelope(99);
            arpEnvelope = new Envelope(0);
            pitchEnvelope = new Envelope(0);
            waveEnvelope = new Envelope(0);
            waveModEnvelope = new Envelope(0);
            waveModType = 0;
            //volumeEnvelope.isActive = true;
            //volumeEnvelope.values.Add(90);
            //volumeEnvelope.values.Add(84);
            //volumeEnvelope.values.Add(78);
            //volumeEnvelope.values.Add(60);
            //volumeEnvelope.releaseIndex = 4;
            //volumeEnvelope.values.Add(60);
            //volumeEnvelope.values.Add(12);
            //volumeEnvelope.values.Add(12);
            //volumeEnvelope.values.Add(12);
            //volumeEnvelope.values.Add(6);
            //volumeEnvelope.values.Add(6);
            //volumeEnvelope.values.Add(6);
            //volumeEnvelope.values.Add(0);

            sample = new Sample();
        }

        public bool IsEqualTo(Instrument other) {
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
        }

        public Instrument Clone() {
            Instrument m = new Instrument(instrumentType);
            //sample.CreateString();
            //m.Unpack(Pack() + sample.stringBuild.ToString());
            m.name = name;
            m.volumeEnvelope = volumeEnvelope.Clone();
            m.pitchEnvelope = pitchEnvelope.Clone();
            m.arpEnvelope = arpEnvelope.Clone();
            m.waveEnvelope = waveEnvelope.Clone();
            m.waveModEnvelope = waveModEnvelope.Clone();
            m.waveModType = waveModType;
            m.sample.sampleDataAccessL = new float[sample.sampleDataAccessL.Length];
            m.sample.sampleDataAccessR = new float[sample.sampleDataAccessR.Length];
            for (int i = 0; i < sample.sampleDataAccessL.Length; i++) {
                m.sample.sampleDataAccessL[i] = sample.sampleDataAccessL[i];
                if (sample.sampleDataAccessR.Length != 0)
                    m.sample.sampleDataAccessR[i] = sample.sampleDataAccessR[i];
            }

            m.sample.sampleLoopType = sample.sampleLoopType;
            m.sample.sampleLoopIndex = sample.sampleLoopIndex;
            m.sample.SetBaseKey(sample.BaseKey);
            m.sample.SetDetune(sample.Detune);
            m.sample.useInVisualization = sample.useInVisualization;
            return m;
        }
        public override string ToString() {
            return name;
        }
        public void SetName(string name) {
            this.name = name;
        }
    }

    [Serializable]
    [ProtoContract(SkipConstructor = false)]
    public class Envelope {
        [ProtoMember(1)]
        public bool isActive = false;
        [ProtoMember(2)]
        public int defaultValue;
        [ProtoMember(3)]
        public List<short> values;
        public int releaseIndex;
        public int loopIndex;
        [ProtoMember(4)]
        byte relIndexSerialized;
        [ProtoMember(5)]
        byte loopIndexSerialized;

        public const int EMPTY_LOOP_RELEASE_INDEX = -1;

        public Envelope Clone() {
            Envelope ret = new Envelope(defaultValue);
            ret.isActive = isActive;
            ret.values = new List<short>();
            for (int i = 0; i < values.Count; i++) {
                ret.values.Add(values[i]);
            }
            ret.loopIndex = loopIndex;
            ret.releaseIndex = releaseIndex;

            return ret;

        }

        [ProtoBeforeSerialization]
        internal void BeforeSerialization() {
            relIndexSerialized = (byte)(releaseIndex + 5);
            loopIndexSerialized = (byte)(loopIndex + 5);
        }
        [ProtoAfterDeserialization]
        internal void AfterDeserialization() {
            releaseIndex = relIndexSerialized - 5;
            loopIndex = loopIndexSerialized - 5;
        }

        public Envelope(int defaultValue) {
            this.defaultValue = defaultValue;
            isActive = false;
            values = new List<short>();
            releaseIndex = EMPTY_LOOP_RELEASE_INDEX;
            loopIndex = EMPTY_LOOP_RELEASE_INDEX;
        }
        public bool IsEqualTo(Envelope other) {
            if (other.isActive != isActive)
                return false;

            if (other.loopIndex != loopIndex)
                return false;
            if (other.releaseIndex != releaseIndex)
                return false;
            if (values.Count != other.values.Count)
                return false;
            for (int i = 0; i < values.Count; ++i) {
                if (values[i] != other.values[i]) {
                    return false;
                }
            }
            return true;
        }

        public bool HasRelease { get { return releaseIndex != EMPTY_LOOP_RELEASE_INDEX; } }
        public bool HasLoop { get { return loopIndex != EMPTY_LOOP_RELEASE_INDEX; } }

        public override string ToString() {
            string s = "";
            if (values.Count > 0) {
                for (int i = 0; i < values.Count - 1; i++) {
                    if (loopIndex == i) s += "| ";
                    if (releaseIndex + 1 == i && releaseIndex >= 0) s += "/ ";
                    s += values[i] + " ";
                }
                int j = values.Count - 1;
                if (loopIndex == j) s += "| ";
                if (releaseIndex + 1 == j && releaseIndex >= 0) s += "/ ";
                s += values[j];
            }
            return s;
        }

        public void LoadFromString(string input, int envelopeType) {
            string[] parts = input.Split(' ');
            int i = 0;
            releaseIndex = -1;
            loopIndex = -1;
            values.Clear();
            foreach (string part in parts) {
                if (part == "/")
                    releaseIndex = --i;
                if (part.Contains('|'))
                    loopIndex = i--;
                short val = 0;
                if (short.TryParse(part, out val)) {
                    if (envelopeType == 0 || envelopeType == 4 || envelopeType == 3) {
                        val = Math.Clamp(val, (short)0, (short)99);
                    }
                    if (envelopeType == 1) {
                        val = Math.Clamp(val, (short)-118, (short)120);
                    }
                    if (envelopeType == 2) {
                        val = Math.Clamp(val, (short)-100, (short)99);
                    }
                }
                values.Add(val);
                i++;
            }
        }
    }
    
    public enum SampleLoopType {
        OneShot,
        Forward,
        PingPong
    }
}
