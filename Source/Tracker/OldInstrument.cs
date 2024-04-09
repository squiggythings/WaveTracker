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
    public class OldInstrument {
        [ProtoMember(17)]
        public string name;
        [ProtoMember(18)]
        public InstrumentType instrumentType;
        [ProtoMember(19)]
        public OldEnvelope volumeEnvelope;
        [ProtoMember(20)]
        public OldEnvelope arpEnvelope;
        [ProtoMember(21)]
        public OldEnvelope pitchEnvelope;
        [ProtoMember(22)]
        public OldEnvelope waveEnvelope;
        [ProtoMember(24)]
        public OldEnvelope waveModEnvelope;
        [ProtoMember(25)]
        public int waveModType;

        [ProtoMember(23)]
        public OldSample sample;
        public const char delimiter = '%';

        public OldInstrument() {
            instrumentType = InstrumentType.Wave;
            name = "New Instrument";
            volumeEnvelope = new OldEnvelope(99);
            arpEnvelope = new OldEnvelope(0);
            pitchEnvelope = new OldEnvelope(0);
            waveEnvelope = new OldEnvelope(0);
            waveModEnvelope = new OldEnvelope(0);
            waveModType = 0;

            sample = new OldSample();
        }
        public OldInstrument(InstrumentType type) {
            instrumentType = type;
            if (type == InstrumentType.Wave)
                name = "New Wave Instrument";
            else if (type == InstrumentType.Sample)
                name = "New Sample Instrument";
            else
                name = "New Instrument";
            volumeEnvelope = new OldEnvelope(99);
            arpEnvelope = new OldEnvelope(0);
            pitchEnvelope = new OldEnvelope(0);
            waveEnvelope = new OldEnvelope(0);
            waveModEnvelope = new OldEnvelope(0);
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

            sample = new OldSample();
        }

        public Instrument ToNewInstrument() {
            if (instrumentType == InstrumentType.Wave) {
                WaveInstrument ret = new WaveInstrument();
                ret.name = name;
                if (volumeEnvelope.isActive) {
                    ret.envelopes.Add(volumeEnvelope.ToNewEnvelope(Envelope.EnvelopeType.Volume));
                }
                if (arpEnvelope.isActive) {
                    ret.envelopes.Add(arpEnvelope.ToNewEnvelope(Envelope.EnvelopeType.Arpeggio));
                }
                if (pitchEnvelope.isActive) {
                    ret.envelopes.Add(pitchEnvelope.ToNewEnvelope(Envelope.EnvelopeType.Pitch));
                }
                if (waveEnvelope.isActive) {
                    ret.envelopes.Add(pitchEnvelope.ToNewEnvelope(Envelope.EnvelopeType.Wave));
                }
                if (waveModEnvelope.isActive) {
                    switch (waveModType) {
                        case 0:
                            ret.envelopes.Add(pitchEnvelope.ToNewEnvelope(Envelope.EnvelopeType.WaveBlend));
                            break;
                        case 1:
                            ret.envelopes.Add(pitchEnvelope.ToNewEnvelope(Envelope.EnvelopeType.WaveStretch));
                            break;
                        case 2:
                            ret.envelopes.Add(pitchEnvelope.ToNewEnvelope(Envelope.EnvelopeType.WaveSync));
                            break;
                    }
                }
                return ret;
            }
            else {
                SampleInstrument ret = new SampleInstrument();
                ret.name = name;
                if (volumeEnvelope.isActive) {
                    ret.envelopes.Add(volumeEnvelope.ToNewEnvelope(Envelope.EnvelopeType.Volume));
                }
                if (arpEnvelope.isActive) {
                    ret.envelopes.Add(arpEnvelope.ToNewEnvelope(Envelope.EnvelopeType.Arpeggio));
                }
                if (pitchEnvelope.isActive) {
                    ret.envelopes.Add(pitchEnvelope.ToNewEnvelope(Envelope.EnvelopeType.Pitch));
                }
                ret.sample = sample.ToNewSample(this);
                return ret;
            }
        }

        public bool IsEqualTo(OldInstrument other) {
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

        public OldInstrument Clone() {
            OldInstrument m = new OldInstrument(instrumentType);
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
    public class OldEnvelope {
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

        public OldEnvelope Clone() {
            OldEnvelope ret = new OldEnvelope(defaultValue);
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

        public OldEnvelope(int defaultValue) {
            this.defaultValue = defaultValue;
            isActive = false;
            values = new List<short>();
            releaseIndex = EMPTY_LOOP_RELEASE_INDEX;
            loopIndex = EMPTY_LOOP_RELEASE_INDEX;
        }

        public Envelope ToNewEnvelope(Envelope.EnvelopeType type) {
            Envelope ret = new Envelope(type);
            ret.values = new sbyte[values.Count];
            for (int i = 0; i < values.Count; i++) {
                ret.values[i] = (sbyte)values[i];
            }
            ret.IsActive = isActive;
            ret.ReleaseIndex = (byte)releaseIndex;
            ret.LoopIndex = (byte)loopIndex;
            return ret;
        }

        public bool IsEqualTo(OldEnvelope other) {
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



    [Serializable]
    [ProtoContract(SkipConstructor = false)]
    public class OldSample {
        [ProtoMember(1)]
        public ResamplingMode resampleMode;
        [ProtoMember(2)]
        public SampleLoopType sampleLoopType;
        [ProtoMember(3)]
        public int sampleLoopIndex;
        [ProtoMember(4)]
        public int Detune { get; private set; }
        [ProtoMember(5)]
        public int BaseKey { get; private set; }
        [ProtoMember(6)]
        public int currentPlaybackPosition;
        [ProtoMember(7)]
        public bool useInVisualization;

        [ProtoMember(8)]
        public float _baseFrequency { get; private set; }
        [ProtoMember(9)]
        public float[] sampleDataAccessL = new float[0];
        [ProtoMember(10)]
        public float[] sampleDataAccessR = new float[0];

        public OldSample() {
            sampleLoopIndex = 0;

            useInVisualization = false;
            sampleDataAccessL = new float[0];
            sampleDataAccessR = new float[0];
            sampleLoopType = SampleLoopType.OneShot;
            resampleMode = (ResamplingMode)Preferences.profile.defaultResampleSample;
            BaseKey = Preferences.profile.defaultBaseKey;
            Detune = 0;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }

        public void SetDetune(int value) {
            Detune = value;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }

        public void SetBaseKey(int value) {
            BaseKey = value;
            _baseFrequency = Helpers.NoteToFrequency(BaseKey - (Detune / 100f));
        }


        public Sample ToNewSample(OldInstrument parentInstrument) {
            Sample sample = new Sample();
            sample.sampleDataAccessL = new short[sampleDataAccessL.Length];
            for (int i = 0; i < sampleDataAccessL.Length; ++i) {
                sample.sampleDataAccessL[i] = (short)(sampleDataAccessL[i] * short.MaxValue);
            }
            sample.sampleDataAccessR = new short[sampleDataAccessR.Length];
            for (int i = 0; i < sampleDataAccessR.Length; ++i) {
                sample.sampleDataAccessR[i] = (short)(sampleDataAccessR[i] * short.MaxValue);
            }
            sample.resampleMode = resampleMode;
            sample.loopType = (Sample.LoopType)parentInstrument.sample.sampleLoopType;
            sample.SetBaseKey(BaseKey + 12);
            sample.SetDetune(Detune);
            sample.useInVisualization = useInVisualization;
            return sample;
        }

        public void Normalize() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            float max = 0;
            foreach (float sample in sampleDataLeft) {
                if (Math.Abs(sample) > max)
                    max = MathF.Abs(sample);
            }
            if (sampleDataRight.Count != 0) {
                foreach (float sample in sampleDataRight) {
                    if (Math.Abs(sample) > max)
                        max = MathF.Abs(sample);
                }
            }
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] /= max;
                if (sampleDataRight.Count != 0)
                    sampleDataRight[i] /= max;
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void Reverse() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            sampleDataLeft.Reverse();
            if (sampleDataRight.Count != 0)
                sampleDataRight.Reverse();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void FadeIn() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] *= (float)i / sampleDataLeft.Count;
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] *= (float)i / sampleDataLeft.Count;
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void FadeOut() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] *= 1 - (float)i / sampleDataLeft.Count;
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] *= 1 - (float)i / sampleDataLeft.Count;
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void Invert() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] *= -1;
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] *= -1;
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public void Amplify(float val) {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; i++) {
                sampleDataLeft[i] = Math.Clamp(sampleDataLeft[i] * val, -1f, 1f);
                if (sampleDataRight.Count != 0) {
                    sampleDataRight[i] = Math.Clamp(sampleDataRight[i] * val, -1f, 1f);
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }
        public void TrimSilence() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            if (sampleDataLeft.Count > 1000) {
                if (sampleDataRight.Count == 0) {
                    for (int i = 0; i < sampleDataLeft.Count; ++i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                    }
                }
                else {
                    for (int i = 0; i < sampleDataLeft.Count; ++i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                        sampleDataRight.RemoveAt(i);
                    }
                    for (int i = sampleDataLeft.Count - 1; i >= 0; --i) {
                        if (Math.Abs(sampleDataLeft[i]) > 0.001f || Math.Abs(sampleDataRight[i]) > 0.001f) {
                            break;
                        }
                        sampleDataLeft.RemoveAt(i);
                        sampleDataRight.RemoveAt(i);
                    }
                }
            }
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        public float GetMonoSample(float time, float startPercentage) {
            SampleTick(time, 0, startPercentage, out float l, out float r);
            return (l + r) / 2f;
        }

        public void SampleTick(float time, float stereoPhase, float startPercentage, out float outputL, out float outputR) {
            float sampleIndex = 0;
            float x = (time * (AudioEngine.SAMPLE_RATE / _baseFrequency));
            x += startPercentage * sampleDataAccessL.Length;
            long l = sampleDataAccessL.Length;
            long p = sampleLoopIndex;
            if (sampleLoopType == SampleLoopType.OneShot || x <= l) {
                sampleIndex = x;
            }
            else if (sampleLoopType == SampleLoopType.PingPong) {
                float b = ((x - p) % ((l - p) * 2));
                if (b < l - p) {
                    sampleIndex = b + p;
                }
                else if (b >= l - p) {
                    sampleIndex = l - (b + p - l);
                }
                //sampleIndex = Math.Abs((sampleIndex - sampleLoopIndex + (len - 1) - 1) % ((len - 1) * 2) - len) + sampleLoopIndex;

            }
            else if (sampleLoopType == SampleLoopType.Forward) {
                sampleIndex = ((x - p) % (l - p)) + p;
            }
            currentPlaybackPosition = (int)sampleIndex;
            if (resampleMode == ResamplingMode.None) {
                outputL = getSample(0, (int)(sampleIndex + stereoPhase * 100)) * 1.5f;
                outputR = getSample(1, (int)(sampleIndex - stereoPhase * 100)) * 1.5f;
            }
            else if (resampleMode == ResamplingMode.Linear) {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(getSample(0, one + (int)(stereoPhase * 100)), getSample(0, two + (int)(stereoPhase * 100)), by) * 1.5f;
                outputR = MathHelper.Lerp(getSample(1, one - (int)(stereoPhase * 100)), getSample(1, two - (int)(stereoPhase * 100)), by) * 1.5f;
            }
            else {
                int one = (int)sampleIndex;
                int two = one + 1;
                float by = (float)(sampleIndex % 1f);
                outputL = MathHelper.Lerp(getSample(0, one + (int)(stereoPhase * 100)), getSample(0, two + (int)(stereoPhase * 100)), by);
                outputR = MathHelper.Lerp(getSample(1, one - (int)(stereoPhase * 100)), getSample(1, two - (int)(stereoPhase * 100)), by);

                outputL += getSample(0, (int)(sampleIndex + stereoPhase * 100));
                outputR += getSample(1, (int)(sampleIndex - stereoPhase * 100));
                outputL /= 1.33333333f;
                outputR /= 1.33333333f;
            }
        }

        public void MixToMono() {
            List<float> sampleDataLeft = sampleDataAccessL.ToList();
            List<float> sampleDataRight = sampleDataAccessR.ToList();
            for (int i = 0; i < sampleDataLeft.Count; ++i) {
                sampleDataLeft[i] += sampleDataRight[i];
                sampleDataLeft[i] /= 2f;
            }
            sampleDataRight.Clear();
            sampleDataAccessL = sampleDataLeft.ToArray();
            sampleDataAccessR = sampleDataRight.ToArray();
        }

        float getSample(int chan, int index) {
            if (sampleDataAccessL.Length == 0) {
                return 0;
            }
            if (index < 0)
                return 0;
            if (index >= sampleDataAccessL.Length)
                return 0;
            if (index >= sampleDataAccessL.Length)
                index = sampleDataAccessL.Length - 1;
            if (chan == 0 || sampleDataAccessL.Length != sampleDataAccessR.Length)
                return sampleDataAccessL[index];
            else
                return sampleDataAccessR[index];
        }


    }
}
